using System.Security.Claims;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/sellers/me/earnings")]
[Authorize(Roles = Roles.Seller)]
public class SellerEarningsController : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly ISettlementService _service;
    private readonly IUnitOfWork _unitOfWork;

    public SellerEarningsController(ISettlementService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetEarnings()
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        return Ok(await _service.GetEarningsAsync(sellerId.Value));
    }

    [HttpGet("settlements")]
    public async Task<IActionResult> GetSettlements([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        return Ok(await _service.GetHistoryAsync(sellerId.Value, pageNumber, pageSize));
    }

    [HttpGet("settlements/{id:int}/export")]
    public async Task<IActionResult> Export(int id, [FromQuery] int tzOffset = 0)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        // التسوية اللي مش بتاعته بترجع NotFound (من غير ما نكشف وجودها)
        var bytes = await _service.ExportSettlementAsync(id, sellerId.Value, tzOffset);
        if (bytes is null) return NotFound();

        return File(bytes, ExcelContentType, $"settlement-{id}.xlsx");
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