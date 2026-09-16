using ECommerce.Application.DTOs.Seller;

namespace ECommerce.Application.Interfaces.Services;

public interface ISellerService
{
    Task<SellerDto?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int sellerId, UpdateSellerDto dto);
}