using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<(List<Category> Items, int TotalCount)> SearchAsync(CategoryQueryParams queryParams)
    {
        var query = _dbSet.AsQueryable();
        query = query.Visible();
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        query = query.OrderBy(c => c.Name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<List<SellerCategoryDto>> GetBySellerAsync(int sellerId)
     => await _dbSet
         .Where(c => c.CreatedBySellerId == sellerId)
         .OrderBy(c => c.Name)
         .Select(c => new SellerCategoryDto
         {
             Id = c.Id,
             Name = c.Name,
             Description = c.Description,
             ImageUrl = c.ImageUrl,
             ParentCategoryId = c.ParentCategoryId,
             ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : string.Empty,
             ProductsCount = c.Products.Count(),
             IsHiddenBySeller = c.IsHiddenBySeller,
             IsHiddenByAdmin = c.IsHiddenByAdmin
         })
         .ToListAsync();

    public async Task<List<AdminCategoryDto>> GetAdminTreeAsync()
    {
        var stores = await _context.Set<Seller>().AsNoTracking()
            .ToDictionaryAsync(s => s.Id, s => s.StoreName);

        var all = await _dbSet.AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.ImageUrl,
                c.ParentCategoryId,
                c.CreatedBySellerId,
                c.IsHiddenBySeller,   // جديد
                c.IsHiddenByAdmin,    // جديد
                ProductsCount = c.Products.Count()
            })
            .ToListAsync();

        string? StoreOf(int? sellerId) =>
            sellerId.HasValue && stores.TryGetValue(sellerId.Value, out var name) ? name : null;

        return all
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .Select(main => new AdminCategoryDto
            {
                Id = main.Id,
                Name = main.Name,
                Description = main.Description,
                ImageUrl = main.ImageUrl,
                ProductsCount = main.ProductsCount,
                IsHiddenBySeller = main.IsHiddenBySeller,   // جديد
                IsHiddenByAdmin = main.IsHiddenByAdmin,     // جديد
                SubCategories = all
                    .Where(s => s.ParentCategoryId == main.Id)
                    .OrderBy(s => s.Name)
                    .Select(s => new AdminSubCategoryDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ProductsCount = s.ProductsCount,
                        StoreName = StoreOf(s.CreatedBySellerId),
                        IsHiddenBySeller = s.IsHiddenBySeller,   // جديد
                        IsHiddenByAdmin = s.IsHiddenByAdmin      // جديد
                    })
                    .ToList()
            })
            .ToList();
    }
}