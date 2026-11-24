using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class SellerRepository : GenericRepository<Seller>, ISellerRepository
{
    public SellerRepository(AppDbContext context) : base(context) { }

    public async Task<Seller?> GetWithUserAsync(int id)
        => await _dbSet
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    public async Task<List<Seller>> GetAllWithUserAsync()
    => await _dbSet.AsNoTracking().Include(s => s.User).ToListAsync();
}