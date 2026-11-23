using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IPaymentGroupRepository : IGenericRepository<PaymentGroup>
{
    Task<PaymentGroup?> GetWithDetailsAsync(int id);
    Task<List<PaymentGroup>> GetPendingForCustomerAsync(int customerId);
    Task<(List<PaymentGroup> Items, int TotalCount)> GetForAdminAsync(string? filter, string? search, int pageNumber, int pageSize);
    Task<int> CountAwaitingAsync();
}
