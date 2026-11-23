using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class PaymentGroupRepository : GenericRepository<PaymentGroup>, IPaymentGroupRepository
{
    public PaymentGroupRepository(AppDbContext context) : base(context) { }

    private static IQueryable<PaymentGroup> WithDetails(IQueryable<PaymentGroup> query)
        => query
            .Include(g => g.PaymentMethod)
            .Include(g => g.Customer).ThenInclude(c => c.User)
            .Include(g => g.Proofs)
            .Include(g => g.Orders).ThenInclude(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)
            .AsSplitQuery();

    public async Task<PaymentGroup?> GetWithDetailsAsync(int id)
        => await WithDetails(_dbSet).FirstOrDefaultAsync(g => g.Id == id);

    // دفعات العميل اللي لسه في انتظار الدفع وفيها طلب نشط
    public async Task<List<PaymentGroup>> GetPendingForCustomerAsync(int customerId)
        => await _dbSet
            .Include(g => g.PaymentMethod)
            .Include(g => g.Orders)
            .Where(g => g.CustomerId == customerId
                        && g.Status == PaymentGroupStatus.Pending
                        && g.Orders.Any(o => o.Status != OrderStatus.Cancelled))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<(List<PaymentGroup> Items, int TotalCount)> GetForAdminAsync(
        string? filter, string? search, int pageNumber, int pageSize)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        // الدفعة اللي كل طلباتها اتلغت مبتظهرش إلا في "الكل"
        query = filter switch
        {
            "paid" => query.Where(g => g.Status == PaymentGroupStatus.Paid),
            "pending" => query.Where(g => g.Status == PaymentGroupStatus.Pending
                                          && g.Orders.Any(o => o.Status != OrderStatus.Cancelled)),
            "all" => query,
            _ => query.Where(g => g.Status == PaymentGroupStatus.AwaitingConfirmation
                                  && g.Orders.Any(o => o.Status != OrderStatus.Cancelled))
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            if (int.TryParse(term, out var groupId))
            {
                query = query.Where(g => g.Id == groupId);
            }
            else
            {
                query = query.Where(g =>
                    (g.Customer.User.FirstName + " " + g.Customer.User.LastName).ToLower().Contains(term) ||
                    (g.Customer.User.Email != null && g.Customer.User.Email.ToLower().Contains(term)));
            }
        }

        var totalCount = await query.CountAsync();

        // المنتظرة للمراجعة: الأقدم الأول. الباقي: الأحدث الأول
        var ordered = filter is null or "awaiting"
            ? query.OrderBy(g => g.CreatedAt)
            : query.OrderByDescending(g => g.CreatedAt);

        var items = await WithDetails(ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> CountAwaitingAsync()
        => await _dbSet.CountAsync(g => g.Status == PaymentGroupStatus.AwaitingConfirmation
                                        && g.Orders.Any(o => o.Status != OrderStatus.Cancelled));
}