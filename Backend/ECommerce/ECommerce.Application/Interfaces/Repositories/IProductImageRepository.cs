using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductImageRepository : IGenericRepository<ProductImage>
{
    Task<List<ProductImage>> GetByProductAsync(int productId);
}