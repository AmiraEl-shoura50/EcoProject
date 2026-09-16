using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<(List<Order> Items, int TotalCount)> GetByCustomerPagedAsync(int customerId, int pageNumber, int pageSize); // ✅ بدل GetByCustomerAsync
    Task<Order?> GetWithItemsAsync(int orderId);
    Task<IEnumerable<Order>> GetBySellerAsync(int sellerId);
}