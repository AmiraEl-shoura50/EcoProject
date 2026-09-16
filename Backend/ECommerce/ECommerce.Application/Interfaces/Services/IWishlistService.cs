using ECommerce.Application.DTOs.Wishlist;

namespace ECommerce.Application.Interfaces.Services;

public interface IWishlistService
{
    Task<WishlistDto> GetByCustomerIdAsync(int customerId);
    Task<bool> AddItemAsync(int customerId, AddToWishlistDto dto);
    Task<bool> RemoveItemAsync(int customerId, int productId);
}