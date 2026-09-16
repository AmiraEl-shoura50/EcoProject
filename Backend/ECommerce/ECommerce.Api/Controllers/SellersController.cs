using ECommerce.Application.DTOs.Seller;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellersController : ControllerBase
{
    private readonly ISellerService _sellerService;
    private readonly IUnitOfWork _unitOfWork;

    public SellersController(ISellerService sellerService, IUnitOfWork unitOfWork)
    {
        _sellerService = sellerService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var seller = await _sellerService.GetByIdAsync(id);
        if (seller is null) return NotFound();
        return Ok(seller);
    }

    [HttpPut("me")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> UpdateMyStore([FromForm] UpdateSellerDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var updated = await _sellerService.UpdateAsync(sellerId.Value, dto);
        if (!updated) return NotFound();
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