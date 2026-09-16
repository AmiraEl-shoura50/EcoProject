using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = Roles.Customer)]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(ICartService cartService, IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var cart = await _cartService.GetByCustomerIdAsync(customerId.Value);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var added = await _cartService.AddItemAsync(customerId.Value, dto);
        if (!added) return BadRequest("تعذر إضافة المنتج - تأكدي من الكمية والمخزون المتاح");
        return Ok();
    }

    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateItem(int productId, [FromBody] UpdateCartItemDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var updated = await _cartService.UpdateItemAsync(customerId.Value, productId, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var removed = await _cartService.RemoveItemAsync(customerId.Value, productId);
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