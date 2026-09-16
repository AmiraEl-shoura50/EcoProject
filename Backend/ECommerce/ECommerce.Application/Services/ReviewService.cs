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
        var (items, totalCount) = await _unitOfWork.Reviews.GetByProductPagedAsync(productId, pageNumber, pageSize);

        return new PaginatedResultDto<ReviewDto>
        {
            Items = _mapper.Map<List<ReviewDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReviewDto?> CreateAsync(int customerId, CreateReviewDto dto)
    {
        // 1️⃣ ✅ Verified Purchase - نتأكد إن الأوردر ده بتاع نفس العميل، فيه المنتج ده، وحالته Delivered
        var order = await _unitOfWork.Orders.GetWithItemsAsync(dto.OrderId);
        if (order is null || order.CustomerId != customerId) return null;

        if (order.Status != OrderStatus.Delivered) return null;

        var boughtProduct = order.Items.Any(i => i.ProductId == dto.ProductId);
        if (!boughtProduct) return null;

        // 2️⃣ ✅ نتأكد إنه ما قيّمش نفس المنتج بنفس الأوردر ده قبل كده
        var alreadyReviewed = await _unitOfWork.Reviews.HasReviewedAsync(customerId, dto.ProductId, dto.OrderId);
        if (alreadyReviewed) return null;

        var review = new Review
        {
            ProductId = dto.ProductId,
            CustomerId = customerId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // 3️⃣ ✅ نحدث متوسط تقييم المنتج
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is not null)
        {
            var average = await _unitOfWork.Reviews.GetAverageRatingAsync(dto.ProductId);
            product.Rating = (float)average;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        //var created = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
        //return _mapper.Map<ReviewDto>(created);
        var resultDto = _mapper.Map<ReviewDto>(review);
        var customer = await _unitOfWork.Customers.GetWithUserAsync(customerId);
        resultDto.CustomerName = customer is not null ? $"{customer.User.FirstName} {customer.User.LastName}" : string.Empty;

        return resultDto;
    }
}