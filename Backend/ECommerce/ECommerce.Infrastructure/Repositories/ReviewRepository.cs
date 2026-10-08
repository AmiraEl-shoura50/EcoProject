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

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        var ratings = await _dbSet.Where(r => r.ProductId == productId).Select(r => r.Rating).ToListAsync();
        return ratings.Any() ? ratings.Average() : 0;
    }
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
}