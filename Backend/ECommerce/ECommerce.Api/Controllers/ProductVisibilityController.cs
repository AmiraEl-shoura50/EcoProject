using System.Security.Claims;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductVisibilityController : ControllerBase
{
    private readonly IProductVisibilityService _service;
    private readonly IUnitOfWork _unitOfWork;

    public ProductVisibilityController(IProductVisibilityService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpPut("{id:int}/visibility")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> SetVisibility(int id, [FromBody] SetVisibilityDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId)) return Forbid();

        var seller = (await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId)).FirstOrDefault();
        if (seller is null) return Forbid();

        // المنتج مش بتاعه أو مش موجود: نفس الرد، من غير ما نكشف وجوده
        if (!await _service.SetProductHiddenBySellerAsync(id, seller.Id, dto.IsHidden)) return NotFound();
        return NoContent();
    }
}