using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<(List<Review> Items, int TotalCount)> GetByProductPagedAsync(int productId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasReviewedAsync(int customerId, int productId, int orderId)
        => await _dbSet.AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId && r.OrderId == orderId);

   
    public async Task<List<Review>> GetLatestAsync(int count)
    {
        return await _dbSet
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Include(r => r.Product)
            .Where(r => r.Rating >= 4) // ✅ بس التقييمات الإيجابية (4 أو 5 نجوم) نعرضها في الصفحة الرئيسية
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    public async Task<double> GetAverageRatingAsync(int productId)
    => await _dbSet
        .Where(r => r.ProductId == productId)
        .AverageAsync(r => (double?)r.Rating) ?? 0;

    public async Task<Dictionary<int, int>> GetDistributionAsync(int productId)
        => await _dbSet
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count);

    public async Task<List<(int OrderId, int ProductId)>> GetReviewedItemsAsync(
        int customerId, IReadOnlyCollection<int> orderIds)
    {
        var rows = await _dbSet
            .Where(r => r.CustomerId == customerId && orderIds.Contains(r.OrderId))
            .Select(r => new { r.OrderId, r.ProductId })
            .ToListAsync();

        return rows.Select(r => (r.OrderId, r.ProductId)).ToList();
    }
}