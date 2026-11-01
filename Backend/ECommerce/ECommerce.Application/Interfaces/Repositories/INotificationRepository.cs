using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<(List<Notification> Items, int TotalCount)> GetByUserPagedAsync(Guid userId, int pageNumber, int pageSize);
    Task<int> CountUnreadAsync(Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<int> DeleteReadAsync(Guid userId);
}