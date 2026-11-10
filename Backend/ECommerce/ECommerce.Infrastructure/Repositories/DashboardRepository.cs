using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    // الحالات اللي الدفع فيها اتأكد: دي اللي بتتحسب كمبيعات
    private static readonly OrderStatus[] PaidStatuses =
    {
        OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered
    };

    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    // ================= البائع =================

    public async Task<Dictionary<OrderStatus, int>> CountSellerOrdersByStatusAsync(int sellerId)
        => await _context.Orders
            .Where(o => !o.IsDeletedBySeller && o.Items.Any(i => i.Product.SellerId == sellerId))
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

    public async Task<int> CountSellerOrdersToReviewAsync(int sellerId)
        => await _context.Orders.CountAsync(o =>
            o.Status == OrderStatus.AwaitingConfirmation &&
            o.PaymentProofs.Any(p => p.IsApproved == null) &&
            o.Items.Any(i => i.Product.SellerId == sellerId));

    public async Task<Dictionary<OrderStatus, decimal>> SumSellerRevenueByStatusAsync(int sellerId)
        => await _context.OrderItems
            .Where(i => i.Product.SellerId == sellerId && PaidStatuses.Contains(i.Order.Status))
            .GroupBy(i => i.Order.Status)
            .Select(g => new { Status = g.Key, Revenue = g.Sum(i => i.UnitPrice * i.Quantity) })
            .ToDictionaryAsync(x => x.Status, x => x.Revenue);

    public async Task<List<(DateTime CreatedDate, decimal Amount)>> GetSellerPaidSalesAsync(int sellerId, DateTime sinceUtc)
    {
        var rows = await _context.OrderItems
            .Where(i => i.Product.SellerId == sellerId
                        && PaidStatuses.Contains(i.Order.Status)
                        && i.Order.CreatedDate >= sinceUtc)
            .Select(i => new { i.Order.CreatedDate, Amount = i.UnitPrice * i.Quantity })
            .ToListAsync();

        return rows.Select(r => (r.CreatedDate, r.Amount)).ToList();
    }

    public async Task<List<TopProductDto>> GetSellerTopProductsAsync(int sellerId, DateTime sinceUtc, int take)
        => await _context.OrderItems
            .Where(i => i.Product.SellerId == sellerId
                        && PaidStatuses.Contains(i.Order.Status)
                        && i.Order.CreatedDate >= sinceUtc)
            .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.ImageUrl })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(p => p.QuantitySold)
            .ThenByDescending(p => p.Revenue)
            .Take(take)
            .ToListAsync();

    // ================= المنصة =================

    public async Task<Dictionary<OrderStatus, int>> CountOrdersByStatusAsync()
        => await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

    public async Task<Dictionary<OrderStatus, decimal>> SumPaidRevenueByStatusAsync()
        => await _context.Orders
            .Where(o => PaidStatuses.Contains(o.Status))
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
            .ToDictionaryAsync(x => x.Status, x => x.Revenue);

    public async Task<List<(DateTime CreatedDate, decimal Amount)>> GetPaidOrdersAsync(DateTime sinceUtc)
    {
        var rows = await _context.Orders
            .Where(o => PaidStatuses.Contains(o.Status) && o.CreatedDate >= sinceUtc)
            .Select(o => new { o.CreatedDate, Amount = o.TotalAmount })
            .ToListAsync();

        return rows.Select(r => (r.CreatedDate, r.Amount)).ToList();
    }

    public async Task<List<TopSellerDto>> GetTopSellersAsync(DateTime sinceUtc, int take)
        => await _context.OrderItems
            .Where(i => PaidStatuses.Contains(i.Order.Status) && i.Order.CreatedDate >= sinceUtc)
            .GroupBy(i => new { i.Product.SellerId, i.Product.Seller.StoreName, i.Product.Seller.ImageUrl })
            .Select(g => new TopSellerDto
            {
                SellerId = g.Key.SellerId,
                StoreName = g.Key.StoreName,
                ImageUrl = g.Key.ImageUrl,
                OrdersCount = g.Select(i => i.OrderId).Distinct().Count(),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(s => s.Revenue)
            .Take(take)
            .ToListAsync();

    public async Task<int> CountUsersAsync()
        => await _context.Users.CountAsync();

    public async Task<int> CountSuspendedUsersAsync()
        => await _context.Users.CountAsync(u => !u.IsActive);

    public async Task<Dictionary<string, int>> CountUsersByRoleAsync()
    {
        var rows = await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          group r by r.Name into g
                          select new { Role = g.Key, Count = g.Count() })
                         .ToListAsync();

        return rows.Where(x => x.Role != null).ToDictionary(x => x.Role!, x => x.Count);
    }

    public async Task<int> CountProductsAsync()
        => await _context.Products.CountAsync();

    public async Task<int> CountCategoriesAsync()
        => await _context.Categories.CountAsync();
}