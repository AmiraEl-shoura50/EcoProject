using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<(List<Review> Items, int TotalCount)> GetByProductPagedAsync(int productId, int pageNumber, int pageSize);
    Task<bool> HasReviewedAsync(int customerId, int productId, int orderId);
    Task<double> GetAverageRatingAsync(int productId);

    Task<List<Review>> GetLatestAsync(int count);
}