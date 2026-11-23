using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(AppDbContext context) : base(context) { }

    public async Task<List<ProductImage>> GetByProductAsync(int productId)
        => await _dbSet
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .ToListAsync();
}