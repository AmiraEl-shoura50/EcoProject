using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId); // ✅ sellerId بقى بارامتر منفصل
    Task<bool> UpdateAsync(int id, UpdateProductDto dto, int sellerId); // ✅ كمان هنا عشان نتأكد إنه صاحب المنتج
    Task<bool> DeleteAsync(int id, int sellerId); // ✅ ونفس الحاجة هنا

    Task<PaginatedResultDto<ProductDto>> SearchAsync(ProductQueryParams queryParams);
}