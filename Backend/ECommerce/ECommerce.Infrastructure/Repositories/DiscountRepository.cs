using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class DiscountRepository : GenericRepository<Discount>, IDiscountRepository
{
    public DiscountRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Discount>> GetAllWithTargetAsync()
        => await _dbSet.Include(d => d.DiscountTarget).ToListAsync();

    public async Task<Discount?> GetByIdWithTargetAsync(int id)
        => await _dbSet.Include(d => d.DiscountTarget).FirstOrDefaultAsync(d => d.Id == id);

    // ✅ بترجع الخصومات الفعالة (النشطة تاريخيًا) على منتج معين، سواء كانت مطبقة عليه مباشرة أو على القسم بتاعه
    public async Task<IEnumerable<Discount>> GetActiveForProductAsync(int productId, int categoryId)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Include(d => d.DiscountTarget)
            .Where(d => d.StartDate <= now && d.EndDate >= now &&
                ((d.DiscountTarget.Target == TargetType.Product && d.DiscountTarget.TargetId == productId) ||
                 (d.DiscountTarget.Target == TargetType.Category && d.DiscountTarget.TargetId == categoryId)))
            .ToListAsync();
    }
}