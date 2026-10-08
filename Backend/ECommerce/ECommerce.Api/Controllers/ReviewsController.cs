using System.Security.Claims;
using ECommerce.Application.DTOs.Review;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IReviewService reviewService, IUnitOfWork unitOfWork)
    {
        _reviewService = reviewService;
        _unitOfWork = unitOfWork;
    }
    [HttpGet("latest")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLatest([FromQuery] int count = 6)
    {
        var reviews = await _reviewService.GetLatestAsync(count);
        return Ok(reviews);
    }
    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetByProductAsync(productId, pageNumber, pageSize);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var created = await _reviewService.CreateAsync(customerId.Value, dto);
        if (created is null)
            return BadRequest("لا يمكن إضافة التقييم - تأكد أن الطلب مكتمل وأنك اشتريت هذا المنتج ولم تقيّميه من قبل");

        return Ok(created);
    }

    private async Task<int?> GetCurrentCustomerIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var customers = await _unitOfWork.Customers.FindAsync(c => c.UserId == userId);
        return customers.FirstOrDefault()?.Id;
    }

}
