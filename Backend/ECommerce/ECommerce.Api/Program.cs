
using ECommerce.API.Filters;
using ECommerce.API.Hubs;
using ECommerce.API.Realtime;
using ECommerce.Application;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;
using FluentValidation.AspNetCore;
using System.Threading.RateLimiting;


namespace E_commerce
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseWrapperFilter>(); // ✅ جديد
            });
            builder.Services.AddFluentValidationAutoValidation();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();
            ////
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
           ////
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();
            ////
            builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            builder.Services.AddSignalR();
            ////
            builder.Services.AddExceptionHandler<ECommerce.API.Middlewares.GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(); // ✅ Fallback format قياسي لو حصل حاجة غريبة قبل ما الـ Handler يشتغل


            var angularOrigins = new[] { "http://localhost:4200", "https://localhost:4200" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins(angularOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // ✅ مهم لو هنستخدم SignalR أو Cookies لاحقًا
                });
            });



            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        data = (object?)null,
                        message = "عدد الطلبات كتير جدًا، برجاء المحاولة بعد قليل"
                    }, cancellationToken);
                };
                // ✅ لما حد يتجاوز الحد، نرجعله 429 بدل ما الـ Request توقف بصمت
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // ✅ سياسة عامة لكل الـ API - حسب IP العميل
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0 // ✅ لو اتجاوز الحد، نرفض فورًا من غير ما نستنى في طابور
                    });

                });

                // ✅ سياسة صارمة خاصة بالـ Auth (Login/Register)
                options.AddPolicy("AuthPolicy", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5, // ✅ 5 محاولات بس
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
                });
            });

            var app = builder.Build();

            app.UseExceptionHandler();
            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAsync(app.Services);
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();
            app.UseCors("AllowAngularApp");
            app.UseRateLimiter(); // ✅ جديد
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");
            await app.RunAsync();
        }
    }
}
