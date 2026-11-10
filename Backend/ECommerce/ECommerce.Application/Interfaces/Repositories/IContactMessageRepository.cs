using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IContactMessageRepository : IGenericRepository<ContactMessage>
{
    Task<(List<ContactMessage> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool unreadOnly, int pageNumber, int pageSize);

    Task<int> CountUnreadAsync();
    Task<int> MarkAllAsReadAsync();
}