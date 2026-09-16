using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IWishlistRepository : IGenericRepository<Wishlist>
{
    Task<Wishlist?> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<WishlistItem>> GetCustomersWhoWishlistedProductAsync(int productId);
}