using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySellerAsync(int sellerId);
    Task<Product?> GetWithDetailsAsync(int id); // بيرجع المنتج مع الـ Category والـ Seller (Include)

    Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryParams queryParams);
}