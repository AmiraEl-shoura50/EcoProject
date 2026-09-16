using System.Security.Claims;
using ECommerce.Application.DTOs.Discount;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;
    private readonly IUnitOfWork _unitOfWork;

    public DiscountsController(IDiscountService discountService, IUnitOfWork unitOfWork)
    {
        _discountService = discountService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var discounts = await _discountService.GetAllAsync();
        return Ok(discounts);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var discount = await _discountService.GetByIdAsync(id);
        if (discount is null) return NotFound();
        return Ok(discount);
    }

    [HttpGet("product/{productId}/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveForProduct(int productId)
    {
        var discounts = await _discountService.GetActiveDiscountsForProductAsync(productId);
        return Ok(discounts);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var created = await _discountService.CreateAsync(dto, sellerId.Value);
        if (created is null) return BadRequest("بيانات الخصم غير صحيحة أو المنتج/القسم غير موجود");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> Delete(int id)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var deleted = await _discountService.DeleteAsync(id, sellerId.Value);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private async Task<int?> GetCurrentSellerIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var sellers = await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId);
        return sellers.FirstOrDefault()?.Id;
    }
}