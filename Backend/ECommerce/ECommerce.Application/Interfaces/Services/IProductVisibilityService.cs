using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Interfaces.Services;

public enum CategoryVisibilityResult { Ok, NotFound, Forbidden }

public interface IProductVisibilityService
{
    Task<PaginatedResultDto<AdminProductDto>> GetAdminProductsAsync(ProductQueryParams queryParams);
    Task<bool> SetProductHiddenBySellerAsync(int productId, int sellerId, bool isHidden);
    Task<bool> SetProductHiddenByAdminAsync(int productId, bool isHidden);
    Task<CategoryVisibilityResult> SetCategoryHiddenAsync(int categoryId, bool isHidden, bool isAdmin, int? sellerId);
}