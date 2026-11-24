using ECommerce.Application.DTOs.Settlement;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface ISettlementRepository : IGenericRepository<Settlement>
{
    Task<List<UnsettledOrderRow>> GetUnsettledOrderRowsAsync(DateTime cutoffUtc, int? sellerId);
    Task<Dictionary<int, PaidTotal>> GetPaidTotalsAsync(int? sellerId);
    Task<(List<Settlement> Items, int TotalCount)> GetPagedAsync(int? sellerId, int pageNumber, int pageSize);
    Task<Settlement?> GetWithOrdersAsync(int id);
    Task<List<Order>> GetDeliveredOrdersAsync(DateTime fromUtc, DateTime toUtc);
    Task<bool> TryCreateAsync(Settlement settlement, IReadOnlyCollection<int> orderIds);
}