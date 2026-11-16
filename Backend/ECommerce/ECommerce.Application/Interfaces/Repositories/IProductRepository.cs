using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Pricing;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySellerAsync(int sellerId);
    Task<Product?> GetWithDetailsAsync(int id); // بيرجع المنتج مع الـ Category والـ Seller (Include)

    Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryParams queryParams);

    Task<List<ProductPricingInfo>> GetPricingInfoAsync(IReadOnlyCollection<int> sellerIds);
    Task<List<Product>> GetByIdsWithDetailsAsync(IReadOnlyCollection<int> ids);

    Task<(int OutOfStock, int LowStock)> GetStockSummaryForSellerAsync(int sellerId, int lowThreshold);
    Task<List<Product>> GetLowStockForSellerAsync(int sellerId, int lowThreshold, int take);
    Task<int?> GetStockForSellerAsync(int productId, int sellerId);
    Task<int> AddStockAsync(int productId, int sellerId, int delta);
    Task<int> SetStockAsync(int productId, int sellerId, int quantity);
    Task<List<int>> GetUnavailableIdsAsync(IReadOnlyCollection<int> productIds);
    Task<int> CountHiddenBySellerAsync(int sellerId);
    Task<int> CountBySellerAsync(int sellerId, bool visibleOnly = false);
    Task<List<Product>> GetBestSellersAsync(int take, DateTime sinceUtc);

}