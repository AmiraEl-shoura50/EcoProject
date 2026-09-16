using ECommerce.Application.DTOs.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce.API.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
     HttpContext httpContext,
     Exception exception,
     CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "حصل خطأ غير متوقع: {Message}", exception.Message);

        var statusCode = MapExceptionToStatusCode(exception);

        var response = new
        {
            success = false,
            data = _environment.IsDevelopment() ? new { exception = exception.ToString() } : null,
            message = GetUserFriendlyMessage(exception, statusCode)
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static int MapExceptionToStatusCode(Exception exception) => exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ArgumentException => StatusCodes.Status400BadRequest,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private string GetUserFriendlyMessage(Exception exception, int statusCode)
    {
        // ✅ في الـ Production، رسالة عامة بس للأخطاء الغير متوقعة (500)
        if (statusCode == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment())
        {
            return "حدث خطأ غير متوقع، برجاء المحاولة لاحقًا";
        }

        return exception.Message;
    }
}