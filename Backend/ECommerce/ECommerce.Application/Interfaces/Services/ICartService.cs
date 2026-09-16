using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartDto> GetByCustomerIdAsync(int customerId);
    Task<bool> AddItemAsync(int customerId, AddToCartDto dto);
    Task<bool> UpdateItemAsync(int customerId, int productId, UpdateCartItemDto dto);
    Task<bool> RemoveItemAsync(int customerId, int productId);
}