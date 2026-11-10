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
    private readonly IDashboardService _dashboardService;
    private readonly IUnitOfWork _unitOfWork;

    public SellersController(
        ISellerService sellerService,
        IDashboardService dashboardService,
        IUnitOfWork unitOfWork)
    {
        _sellerService = sellerService;
        _dashboardService = dashboardService;
        _unitOfWork = unitOfWork;
    }

    // عام: بيانات المتجر من غير أي معلومات شخصية
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var seller = await _sellerService.GetByIdAsync(id);
        if (seller is null) return NotFound();
        return Ok(seller);
    }

    // خاص: بيانات متجري أنا
    [HttpGet("me")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetMine()
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var seller = await _sellerService.GetMineAsync(sellerId.Value);
        if (seller is null) return NotFound();
        return Ok(seller);
    }

    // نظرة عامة على متجري: tzOffset هو فرق توقيت المتصفح بالدقايق (JavaScript getTimezoneOffset)
    [HttpGet("me/dashboard")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetMyDashboard([FromQuery] int tzOffset = 0)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var dashboard = await _dashboardService.GetSellerAsync(sellerId.Value, Math.Clamp(tzOffset, -840, 840));
        return Ok(dashboard);
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