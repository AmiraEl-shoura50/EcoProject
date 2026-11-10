using ECommerce.Application.DTOs.Seller;

namespace ECommerce.Application.Interfaces.Services;

public interface ISellerService
{
    Task<SellerDto?> GetByIdAsync(int id);
    Task<SellerProfileDto?> GetMineAsync(int sellerId);
    Task<bool> UpdateAsync(int sellerId, UpdateSellerDto dto);
}