using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(AppDbContext context) : base(context) { }

    public async Task<Wishlist?> GetByCustomerIdAsync(int customerId)
        => await _dbSet
            .Include(w => w.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);

    public async Task<IEnumerable<WishlistItem>> GetCustomersWhoWishlistedProductAsync(int productId)
    => await _context.Set<WishlistItem>()
        .Include(wi => wi.Wishlist)
            .ThenInclude(w => w.Customer)
                .ThenInclude(c => c.User)
        .Where(wi => wi.ProductId == productId)
        .ToListAsync();
}