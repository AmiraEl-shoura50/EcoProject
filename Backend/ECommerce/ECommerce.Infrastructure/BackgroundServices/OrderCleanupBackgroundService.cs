using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.BackgroundServices;

public class OrderCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCleanupBackgroundService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan PendingTimeout = TimeSpan.FromHours(24);

    public OrderCleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OrderCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupStaleOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تنظيف الطلبات القديمة");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CleanupStaleOrdersAsync()
    {
        // ✅ محتاجين Scope جديد يدويًا لأن BackgroundService نفسها Singleton
        // لكن IUnitOfWork بتاعنا Scoped، فمينفعش نحقنها مباشرة في الـ Constructor
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoff = DateTime.UtcNow - PendingTimeout;
        var staleOrders = await unitOfWork.Orders.FindAsync(
            o => o.Status == OrderStatus.Pending && o.CreatedDate < cutoff);

        foreach (var order in staleOrders)
        {
            var fullOrder = await unitOfWork.Orders.GetWithItemsAsync(order.Id);
            if (fullOrder is null) continue;

            // ✅ إرجاع الـ Stock
            foreach (var item in fullOrder.Items)
            {
                item.Product.StockQuantity += item.Quantity;
                unitOfWork.Products.Update(item.Product);
            }

            fullOrder.Status = OrderStatus.Cancelled;
            unitOfWork.Orders.Update(fullOrder);

            await notificationService.CreateAndSendAsync(
                fullOrder.Customer.UserId,
                "تم إلغاء الطلب",
                $"تم إلغاء طلبك #{fullOrder.Id} تلقائيًا لعدم تأكيده خلال 24 ساعة");
        }

        await unitOfWork.SaveChangesAsync();

        if (staleOrders.Any())
        {
            _logger.LogInformation("تم إلغاء {Count} طلب معلّق", staleOrders.Count());
        }
    }
}