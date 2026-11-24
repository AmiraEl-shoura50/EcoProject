using ECommerce.Application.DTOs.Settlement;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class SettlementRepository : GenericRepository<Settlement>, ISettlementRepository
{
    public SettlementRepository(AppDbContext context) : base(context) { }

    // الطلبات المتوصّلة اللي لسه ما اتسوّتش (وتاريخ توصيلها قبل cutoffUtc)، طلب لكل بائع
    public async Task<List<UnsettledOrderRow>> GetUnsettledOrderRowsAsync(DateTime cutoffUtc, int? sellerId)
    {
        var query = _context.OrderItems
            .Where(i => i.Order.Status == OrderStatus.Delivered
                        && i.Order.SettlementId == null
                        && (i.Order.CompletedDate ?? i.Order.CreatedDate) < cutoffUtc);

        if (sellerId.HasValue)
            query = query.Where(i => i.Product.SellerId == sellerId.Value);

        var rows = await query
            .GroupBy(i => new { i.OrderId, i.Product.SellerId })
            .Select(g => new
            {
                g.Key.OrderId,
                g.Key.SellerId,
                Amount = g.Sum(i => i.UnitPrice * i.Quantity),
                DeliveredAt = g.Min(i => i.Order.CompletedDate ?? i.Order.CreatedDate)
            })
            .ToListAsync();

        return rows.Select(r => new UnsettledOrderRow(r.OrderId, r.SellerId, r.Amount, r.DeliveredAt)).ToList();
    }

    public async Task<Dictionary<int, PaidTotal>> GetPaidTotalsAsync(int? sellerId)
    {
        var query = _dbSet.AsQueryable();
        if (sellerId.HasValue) query = query.Where(s => s.SellerId == sellerId.Value);

        var rows = await query
            .GroupBy(s => s.SellerId)
            .Select(g => new { SellerId = g.Key, Total = g.Sum(s => s.NetAmount), Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(r => r.SellerId, r => new PaidTotal(r.Total, r.Count));
    }

    public async Task<(List<Settlement> Items, int TotalCount)> GetPagedAsync(int? sellerId, int pageNumber, int pageSize)
    {
        var query = _dbSet.AsNoTracking().Include(s => s.Seller).AsQueryable();
        if (sellerId.HasValue) query = query.Where(s => s.SellerId == sellerId.Value);

        query = query.OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<Settlement?> GetWithOrdersAsync(int id)
        => await _dbSet
            .AsNoTracking()
            .Include(s => s.Seller).ThenInclude(x => x.User)
            .Include(s => s.Orders).ThenInclude(o => o.Items).ThenInclude(i => i.Product)
            .Include(s => s.Orders).ThenInclude(o => o.PaymentMethod)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<List<Order>> GetDeliveredOrdersAsync(DateTime fromUtc, DateTime toUtc)
        => await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller).ThenInclude(s => s.User)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Settlement)
            .Where(o => o.Status == OrderStatus.Delivered
                        && (o.CompletedDate ?? o.CreatedDate) >= fromUtc
                        && (o.CompletedDate ?? o.CreatedDate) < toUtc)
            .OrderBy(o => o.CompletedDate ?? o.CreatedDate)
            .AsSplitQuery()
            .ToListAsync();

    // إنشاء التسوية وقفل الطلبات في معاملة واحدة: لو أي طلب اتسوّى في نفس اللحظة، كل حاجة بتتلغي
    public async Task<bool> TryCreateAsync(Settlement settlement, IReadOnlyCollection<int> orderIds)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _dbSet.AddAsync(settlement);
        await _context.SaveChangesAsync();

        var settlementId = settlement.Id;

        var updated = await _context.Orders
            .Where(o => orderIds.Contains(o.Id) && o.SettlementId == null && o.Status == OrderStatus.Delivered)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.SettlementId, settlementId));

        if (updated != orderIds.Count)
        {
            await transaction.RollbackAsync();
            _context.Entry(settlement).State = EntityState.Detached;
            return false;
        }

        await transaction.CommitAsync();
        return true;
    }
}