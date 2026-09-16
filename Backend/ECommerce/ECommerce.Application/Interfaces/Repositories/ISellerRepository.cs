using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface ISellerRepository : IGenericRepository<Seller>
{
    Task<Seller?> GetWithUserAsync(int id);
}