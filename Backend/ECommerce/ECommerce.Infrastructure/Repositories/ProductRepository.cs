using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Pricing;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
     => await _dbSet
         .Where(p => p.CategoryId == categoryId || p.Category.ParentCategoryId == categoryId)
         .ToListAsync();

    public async Task<IEnumerable<Product>> GetBySellerAsync(int sellerId)
        => await _dbSet.Where(p => p.SellerId == sellerId).ToListAsync();

    public async Task<Product?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryParams queryParams)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .AsQueryable();

        // ✅ فلترة بالبحث (في الاسم أو الوصف)
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        // ✅ فلترة بالقسم (القسم نفسه + أقسامه الفرعية لو كان رئيسي)
        if (queryParams.CategoryId.HasValue)
        {
            var categoryId = queryParams.CategoryId.Value;
            query = query.Where(p => p.CategoryId == categoryId || p.Category.ParentCategoryId == categoryId);
        }
        // ✅ فلترة بالسعر
        if (queryParams.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= queryParams.MinPrice.Value);
        }
        if (queryParams.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);
        }
        // ✅ فلترة بالبائع
        if (queryParams.SellerId.HasValue)
        {
            query = query.Where(p => p.SellerId == queryParams.SellerId.Value);
        }

        // ✅ الترتيب
        query = queryParams.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.Id), // بما إننا مفيش CreatedDate في Product، بنستخدم Id كتقريب
            _ => query.OrderBy(p => p.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<List<ProductPricingInfo>> GetPricingInfoAsync(IReadOnlyCollection<int> sellerIds)
    => await _dbSet
        .Where(p => sellerIds.Contains(p.SellerId))
        .Select(p => new ProductPricingInfo(p.Id, p.CategoryId, p.SellerId, p.Price, p.Rating))
        .ToListAsync();

    public async Task<List<Product>> GetByIdsWithDetailsAsync(IReadOnlyCollection<int> ids)
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
}