using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ContactMessageRepository : GenericRepository<ContactMessage>, IContactMessageRepository
{
    public ContactMessageRepository(AppDbContext context) : base(context) { }

    public async Task<(List<ContactMessage> Items, int TotalCount)> SearchAsync(
        string? searchTerm, bool unreadOnly, int pageNumber, int pageSize)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (unreadOnly)
            query = query.Where(m => !m.IsRead);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();

            query = query.Where(m =>
                m.Name.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term) ||
                m.Subject.ToLower().Contains(term) ||
                m.Message.ToLower().Contains(term));
        }

        query = query.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> CountUnreadAsync()
        => await _dbSet.CountAsync(m => !m.IsRead);

    public async Task<int> MarkAllAsReadAsync()
        => await _dbSet
            .Where(m => !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
}