using System.Security.Claims;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize(Roles = Roles.Customer)]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly IUnitOfWork _unitOfWork;

    public WishlistController(IWishlistService wishlistService, IUnitOfWork unitOfWork)
    {
        _wishlistService = wishlistService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var wishlist = await _wishlistService.GetByCustomerIdAsync(customerId.Value);
        return Ok(wishlist);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToWishlistDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var added = await _wishlistService.AddItemAsync(customerId.Value, dto);
        if (!added) return BadRequest("المنتج غير موجود");
        return Ok();
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var removed = await _wishlistService.RemoveItemAsync(customerId.Value, productId);
        if (!removed) return NotFound();
        return NoContent();
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