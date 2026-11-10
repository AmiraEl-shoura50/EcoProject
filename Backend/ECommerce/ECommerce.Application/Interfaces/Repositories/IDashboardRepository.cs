using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    // ---- البائع ----
    Task<Dictionary<OrderStatus, int>> CountSellerOrdersByStatusAsync(int sellerId);
    Task<int> CountSellerOrdersToReviewAsync(int sellerId);
    Task<Dictionary<OrderStatus, decimal>> SumSellerRevenueByStatusAsync(int sellerId);
    Task<List<(DateTime CreatedDate, decimal Amount)>> GetSellerPaidSalesAsync(int sellerId, DateTime sinceUtc);
    Task<List<TopProductDto>> GetSellerTopProductsAsync(int sellerId, DateTime sinceUtc, int take);

    // ---- المنصة (الأدمن) ----
    Task<Dictionary<OrderStatus, int>> CountOrdersByStatusAsync();
    Task<Dictionary<OrderStatus, decimal>> SumPaidRevenueByStatusAsync();
    Task<List<(DateTime CreatedDate, decimal Amount)>> GetPaidOrdersAsync(DateTime sinceUtc);
    Task<List<TopSellerDto>> GetTopSellersAsync(DateTime sinceUtc, int take);
    Task<int> CountUsersAsync();
    Task<int> CountSuspendedUsersAsync();
    Task<Dictionary<string, int>> CountUsersByRoleAsync();
    Task<int> CountProductsAsync();
    Task<int> CountCategoriesAsync();
}