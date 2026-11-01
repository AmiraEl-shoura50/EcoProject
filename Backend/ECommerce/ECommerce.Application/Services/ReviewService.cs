using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Review;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<ReviewDto>> GetByProductAsync(int productId, int pageNumber, int pageSize)
    {
        // ✅ حد أقصى للصفحة عشان محدش يطلب الجدول كله
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await _unitOfWork.Reviews.GetByProductPagedAsync(productId, pageNumber, pageSize);

        return new PaginatedResultDto<ReviewDto>
        {
            Items = _mapper.Map<List<ReviewDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReviewSummaryDto> GetSummaryAsync(int productId)
    {
        var distribution = await _unitOfWork.Reviews.GetDistributionAsync(productId);
        var total = distribution.Values.Sum();

        var average = total == 0
            ? 0
            : Math.Round(distribution.Sum(x => (double)x.Key * x.Value) / total, 1);

        return new ReviewSummaryDto
        {
            Average = average,
            TotalCount = total,
            Distribution = Enumerable.Range(1, 5).Select(star => distribution.GetValueOrDefault(star)).ToArray()
        };
    }

    public async Task<IEnumerable<LatestReviewDto>> GetLatestAsync(int count)
    {
        var reviews = await _unitOfWork.Reviews.GetLatestAsync(count);

        return reviews.Select(r => new LatestReviewDto
        {
            CustomerName = $"{r.Customer.User.FirstName} {r.Customer.User.LastName}",
            ProductName = r.Product.Name,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<ReviewDto?> CreateAsync(int customerId, CreateReviewDto dto)
    {
        // ✅ شراء موثّق: الطلب بتاع نفس العميل، متوصّل، وفيه المنتج ده
        var order = await _unitOfWork.Orders.GetWithItemsAsync(dto.OrderId);
        if (order is null || order.CustomerId != customerId) return null;
        if (order.Status != OrderStatus.Delivered) return null;
        if (!order.Items.Any(i => i.ProductId == dto.ProductId)) return null;

        // ✅ مرة واحدة لكل منتج في كل طلب
        if (await _unitOfWork.Reviews.HasReviewedAsync(customerId, dto.ProductId, dto.OrderId)) return null;

        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is null) return null;

        var review = new Review
        {
            ProductId = dto.ProductId,
            CustomerId = customerId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(review);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception)
        {
            // سباق نادر: ضغطتين متزامنتين على نفس التقييم، والفهرس الفريد منع التانية
            if (await _unitOfWork.Reviews.HasReviewedAsync(customerId, dto.ProductId, dto.OrderId)) return null;
            throw;
        }

        // ✅ المتوسط والعدد بيتحسبوا من الداتابيز بعد الحفظ، فبيصحّحوا نفسهم حتى لو حصل تزامن
        var distribution = await _unitOfWork.Reviews.GetDistributionAsync(dto.ProductId);
        var count = distribution.Values.Sum();

        product.Rating = count == 0
            ? 0
            : (float)Math.Round(distribution.Sum(x => (double)x.Key * x.Value) / count, 1);
        product.ReviewsCount = count;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = _mapper.Map<ReviewDto>(review);
        var customer = await _unitOfWork.Customers.GetWithUserAsync(customerId);
        resultDto.CustomerName = customer is not null
            ? $"{customer.User.FirstName} {customer.User.LastName}"
            : string.Empty;

        return resultDto;
    }
}