using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IDiscountRepository : IGenericRepository<Discount>
{
    Task<IEnumerable<Discount>> GetAllWithTargetAsync();
    Task<Discount?> GetByIdWithTargetAsync(int id);
    Task<IEnumerable<Discount>> GetActiveForProductAsync(int productId, int categoryId);
}