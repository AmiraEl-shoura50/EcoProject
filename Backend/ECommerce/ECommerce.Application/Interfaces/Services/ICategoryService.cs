using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    // Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<PaginatedResultDto<CategoryDto>> SearchAsync(CategoryQueryParams queryParams); // ✅ بدل GetAllAsync
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}