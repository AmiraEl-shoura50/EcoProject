using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Review;

namespace ECommerce.Application.Interfaces.Services;

public interface IReviewService
{
    Task<PaginatedResultDto<ReviewDto>> GetByProductAsync(int productId, int pageNumber, int pageSize);
    Task<ReviewSummaryDto> GetSummaryAsync(int productId);
    Task<IEnumerable<LatestReviewDto>> GetLatestAsync(int count);
    Task<ReviewDto?> CreateAsync(int customerId, CreateReviewDto dto);
}