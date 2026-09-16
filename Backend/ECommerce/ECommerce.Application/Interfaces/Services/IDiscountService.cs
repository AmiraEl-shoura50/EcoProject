using ECommerce.Application.DTOs.Discount;

namespace ECommerce.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<IEnumerable<DiscountDto>> GetAllAsync();
    Task<DiscountDto?> GetByIdAsync(int id);
    Task<DiscountDto?> CreateAsync(CreateDiscountDto dto, int sellerId);
    Task<bool> DeleteAsync(int id, int sellerId);
    Task<IEnumerable<DiscountDto>> GetActiveDiscountsForProductAsync(int productId);
}