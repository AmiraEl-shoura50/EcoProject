using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<(List<Notification> Items, int TotalCount)> GetByUserPagedAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> CountUnreadAsync(Guid userId)
        => await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<int> MarkAllAsReadAsync(Guid userId)
        => await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    public async Task<int> DeleteReadAsync(Guid userId)
    => await _dbSet
        .Where(n => n.UserId == userId && n.IsRead)
        .ExecuteDeleteAsync();
}