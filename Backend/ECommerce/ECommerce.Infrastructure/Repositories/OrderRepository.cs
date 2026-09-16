using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<(List<Order> Items, int TotalCount)> GetByCustomerPagedAsync(int customerId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.PaymentMethod)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<Order?> GetWithItemsAsync(int orderId)
        => await _dbSet
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.PaymentMethod)
            .Include(o => o.PaymentProofs)
            .Include(o => o.PaymentTransactions)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<IEnumerable<Order>> GetBySellerAsync(int sellerId)
        => await _dbSet
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.PaymentMethod)
            .Include(o => o.PaymentProofs)
            .Where(o => o.Items.Any(i => i.Product.SellerId == sellerId))
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
}