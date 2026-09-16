using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Review;

namespace ECommerce.Application.Interfaces.Services;

public interface IReviewService
{
    Task<PaginatedResultDto<ReviewDto>> GetByProductAsync(int productId, int pageNumber, int pageSize);
    Task<ReviewDto?> CreateAsync(int customerId, CreateReviewDto dto);
}