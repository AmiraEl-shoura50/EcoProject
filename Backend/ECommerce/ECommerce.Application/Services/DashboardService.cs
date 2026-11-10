using System.Globalization;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Enums;
using ECommerce.Application.Constants;  
namespace ECommerce.Application.Services;

public class DashboardService : IDashboardService
{
 
    private const int ChartDays = 30;

    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SellerDashboardDto> GetSellerAsync(int sellerId, int tzOffsetMinutes)
    {
        var (shift, localToday, sinceUtc) = Window(tzOffsetMinutes);
        var dashboard = _unitOfWork.Dashboard;

        var ordersByStatus = await dashboard.CountSellerOrdersByStatusAsync(sellerId);
        var toReview = await dashboard.CountSellerOrdersToReviewAsync(sellerId);
        var revenueByStatus = await dashboard.SumSellerRevenueByStatusAsync(sellerId);
        var sales = await dashboard.GetSellerPaidSalesAsync(sellerId, sinceUtc);
        var topProducts = await dashboard.GetSellerTopProductsAsync(sellerId, sinceUtc, 5);

        var (outOfStock, lowStock) = await _unitOfWork.Products.GetStockSummaryForSellerAsync(sellerId, StockRules.LowStockThreshold);
        var lowStockProducts = await _unitOfWork.Products.GetLowStockForSellerAsync(sellerId, StockRules.LowStockThreshold, 5);
        var productsCount = await _unitOfWork.Products.CountBySellerAsync(sellerId);
        var (rating, reviewsCount) = await _unitOfWork.Reviews.GetSellerStatsAsync(sellerId);

        var daily = BuildDailySeries(sales, shift, localToday);

        return new SellerDashboardDto
        {
            TotalSales = Round(revenueByStatus.GetValueOrDefault(OrderStatus.Delivered)),
            InProgressSales = Round(
                revenueByStatus.GetValueOrDefault(OrderStatus.Confirmed) +
                revenueByStatus.GetValueOrDefault(OrderStatus.Shipped)),
            SalesLast30Days = Round(daily.Sum(d => d.Amount)),
            DeliveredOrders = ordersByStatus.GetValueOrDefault(OrderStatus.Delivered),

            OrdersToReview = toReview,
            OrdersToShip = ordersByStatus.GetValueOrDefault(OrderStatus.Confirmed),
            OrdersInTransit = ordersByStatus.GetValueOrDefault(OrderStatus.Shipped),
            OrdersAwaitingPayment = ordersByStatus.GetValueOrDefault(OrderStatus.Pending),

            ProductsCount = productsCount,
            OutOfStockCount = outOfStock,
            LowStockCount = lowStock,

            Rating = rating,
            ReviewsCount = reviewsCount,

            DailySales = daily,
            TopProducts = topProducts,
            LowStockProducts = lowStockProducts.Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity
            }).ToList()
        };
    }

    public async Task<AdminDashboardDto> GetAdminAsync(int tzOffsetMinutes)
    {
        var (shift, localToday, sinceUtc) = Window(tzOffsetMinutes);
        var dashboard = _unitOfWork.Dashboard;

        var ordersByStatus = await dashboard.CountOrdersByStatusAsync();
        var revenueByStatus = await dashboard.SumPaidRevenueByStatusAsync();
        var paidOrders = await dashboard.GetPaidOrdersAsync(sinceUtc);
        var topSellers = await dashboard.GetTopSellersAsync(sinceUtc, 5);

        var totalUsers = await dashboard.CountUsersAsync();
        var suspendedUsers = await dashboard.CountSuspendedUsersAsync();
        var usersByRole = await dashboard.CountUsersByRoleAsync();
        var productsCount = await dashboard.CountProductsAsync();
        var categoriesCount = await dashboard.CountCategoriesAsync();

        var daily = BuildDailySeries(paidOrders, shift, localToday);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            CustomersCount = usersByRole.GetValueOrDefault("Customer"),
            SellersCount = usersByRole.GetValueOrDefault("Seller"),
            SuspendedUsers = suspendedUsers,

            ProductsCount = productsCount,
            CategoriesCount = categoriesCount,

            TotalOrders = ordersByStatus.Values.Sum(),
            OrdersByStatus = ordersByStatus.ToDictionary(x => x.Key.ToString(), x => x.Value),
            TotalSales = Round(revenueByStatus.Values.Sum()),
            SalesLast30Days = Round(daily.Sum(d => d.Amount)),

            DailySales = daily,
            TopSellers = topSellers
        };
    }

    // نافذة آخر 30 يوم بتوقيت المستخدم: Shift = (توقيت المستخدم - UTC)
    private static (TimeSpan Shift, DateTime LocalToday, DateTime SinceUtc) Window(int tzOffsetMinutes)
    {
        var shift = TimeSpan.FromMinutes(-tzOffsetMinutes);
        var localToday = (DateTime.UtcNow + shift).Date;
        var sinceUtc = localToday.AddDays(-(ChartDays - 1)) - shift;

        return (shift, localToday, sinceUtc);
    }

    // سلسلة أيام كاملة (الأيام اللي مفيهاش مبيعات بتتملى بصفر)
    private static List<DailySalesDto> BuildDailySeries(
        IEnumerable<(DateTime CreatedDate, decimal Amount)> rows, TimeSpan shift, DateTime localToday)
    {
        var totals = rows
            .GroupBy(r => (r.CreatedDate + shift).Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        return Enumerable.Range(0, ChartDays)
            .Select(i => localToday.AddDays(i - (ChartDays - 1)))
            .Select(day => new DailySalesDto
            {
                Date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Amount = Round(totals.GetValueOrDefault(day))
            })
            .ToList();
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}