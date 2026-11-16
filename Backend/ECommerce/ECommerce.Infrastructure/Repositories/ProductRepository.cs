using ECommerce.Application.Constants;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Pricing;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        => await _dbSet.Where(p => p.CategoryId == categoryId).Visible().ToListAsync();

    public async Task<IEnumerable<Product>> GetBySellerAsync(int sellerId)
        => await _dbSet.Where(p => p.SellerId == sellerId).ToListAsync();

    public async Task<Product?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(p => p.Category).ThenInclude(c => c.ParentCategory)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryParams queryParams)
    {
        var query = _dbSet
            .Include(p => p.Category).ThenInclude(c => c.ParentCategory)
            .Include(p => p.Seller)
            .AsQueryable();

        // ✅ الزوار والعملاء ميشوفوش المنتجات المخفية (البائع والأدمن بيطلبوها بـ IncludeHidden)
        if (!queryParams.IncludeHidden)
            query = query.Visible();

        // فلتر الإخفاء (للبائع والأدمن)
        if (queryParams.VisibilityFilter == "hidden")
            query = query.Where(p => p.IsHiddenBySeller || p.IsHiddenByAdmin);
        else if (queryParams.VisibilityFilter == "visible")
            query = query.Where(p => !p.IsHiddenBySeller && !p.IsHiddenByAdmin);

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

        // ✅ فلتر المخزون (للبائع): نفذت / قاربت على النفاد / الاتنين
        if (!string.IsNullOrWhiteSpace(queryParams.StockFilter))
        {
            var threshold = StockRules.LowStockThreshold;

            query = queryParams.StockFilter.ToLowerInvariant() switch
            {
                "out" => query.Where(p => p.StockQuantity <= 0),
                "low" => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= threshold),
                "attention" => query.Where(p => p.StockQuantity <= threshold),
                _ => query
            };
        }

        // ✅ الترتيب
        query = queryParams.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.Id), // بما إننا مفيش CreatedDate في Product، بنستخدم Id كتقريب
            "stock_asc" => query.OrderBy(p => p.StockQuantity).ThenBy(p => p.Id),
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
            .Visible()
            .Select(p => new ProductPricingInfo(p.Id, p.CategoryId, p.SellerId, p.Price, p.Rating))
            .ToListAsync();

    public async Task<List<Product>> GetByIdsWithDetailsAsync(IReadOnlyCollection<int> ids)
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Where(p => ids.Contains(p.Id))
            .Visible()
            .ToListAsync();

    public async Task<(int OutOfStock, int LowStock)> GetStockSummaryForSellerAsync(int sellerId, int lowThreshold)
    {
        var outOfStock = await _dbSet.CountAsync(p => p.SellerId == sellerId && p.StockQuantity <= 0);
        var lowStock = await _dbSet.CountAsync(p =>
            p.SellerId == sellerId && p.StockQuantity > 0 && p.StockQuantity <= lowThreshold);

        return (outOfStock, lowStock);
    }

    public async Task<List<Product>> GetLowStockForSellerAsync(int sellerId, int lowThreshold, int take)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId && p.StockQuantity <= lowThreshold)
            .OrderBy(p => p.StockQuantity).ThenBy(p => p.Name)
            .Take(take)
            .ToListAsync();

    // null لو المنتج مش موجود أو مش بتاع البائع ده
    public async Task<int?> GetStockForSellerAsync(int productId, int sellerId)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.Id == productId && p.SellerId == sellerId)
            .Select(p => (int?)p.StockQuantity)
            .FirstOrDefaultAsync();

    // الإضافة بتتم جوه الداتابيز (Stock = Stock + X)، فمفيش طلب عميل بيتضيّع لو حصل في نفس اللحظة
    public async Task<int> AddStockAsync(int productId, int sellerId, int delta)
        => await _dbSet
            .Where(p => p.Id == productId && p.SellerId == sellerId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + delta));

    public async Task<int> SetStockAsync(int productId, int sellerId, int quantity)
        => await _dbSet
            .Where(p => p.Id == productId && p.SellerId == sellerId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, quantity));

    public async Task<List<Product>> GetBestSellersAsync(int take, DateTime sinceUtc)
    {
        // الترتيب والتجميع كله في الداتابيز: مجموع القطع المباعة لكل منتج
        var top = await _context.OrderItems
            .Where(i => i.Order.CreatedDate >= sinceUtc
                        && (i.Order.Status == OrderStatus.Confirmed
                            || i.Order.Status == OrderStatus.Shipped
                            || i.Order.Status == OrderStatus.Delivered)
                        && i.Product.StockQuantity > 0
                        && !i.Product.IsHiddenBySeller && !i.Product.IsHiddenByAdmin
                        && !i.Product.Category.IsHiddenBySeller && !i.Product.Category.IsHiddenByAdmin
                        && (i.Product.Category.ParentCategory == null
                            || (!i.Product.Category.ParentCategory.IsHiddenBySeller
                                && !i.Product.Category.ParentCategory.IsHiddenByAdmin)))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Sold = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Sold)
            .ThenBy(x => x.ProductId)
            .Take(take)
            .ToListAsync();

        var ids = top.Select(x => x.ProductId).ToList();

        var products = await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        // نرجّعهم بترتيب الأكثر مبيعًا
        return ids
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .OfType<Product>()
            .ToList();
    }

    public async Task<int> CountBySellerAsync(int sellerId, bool visibleOnly = false)
    {
        var query = _dbSet.Where(p => p.SellerId == sellerId);
        if (visibleOnly) query = query.Visible();

        return await query.CountAsync();
    }

    // أرقام المنتجات (من اللي اتبعتت) اللي مش ظاهرة للعملاء دلوقتي (لأي سبب)
    public async Task<List<int>> GetUnavailableIdsAsync(IReadOnlyCollection<int> productIds)
        => await _dbSet
            .Where(p => productIds.Contains(p.Id))
            .Hidden()
            .Select(p => p.Id)
            .ToListAsync();

    public async Task<int> CountHiddenBySellerAsync(int sellerId)
        => await _dbSet.CountAsync(p => p.SellerId == sellerId && (p.IsHiddenBySeller || p.IsHiddenByAdmin));
} 