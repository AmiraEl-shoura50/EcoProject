using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<PaginatedResultDto<CategoryDto>> SearchAsync(CategoryQueryParams queryParams);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetTopLevelWithChildrenAsync();
    Task<IEnumerable<SellerCategoryDto>> GetMineAsync(int sellerId);
    Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, string userRole, int? sellerId);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto dto, string userRole, int? sellerId);
    Task<DeleteCategoryResult> DeleteAsync(int id, string userRole, int? sellerId);
    Task<IEnumerable<AdminCategoryDto>> GetAdminTreeAsync();
}