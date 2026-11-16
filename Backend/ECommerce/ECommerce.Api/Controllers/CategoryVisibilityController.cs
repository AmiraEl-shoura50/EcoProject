using System.Security.Claims;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryVisibilityController : ControllerBase
{
    private readonly IProductVisibilityService _service;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryVisibilityController(IProductVisibilityService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpPut("{id:int}/visibility")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Seller}")]
    public async Task<IActionResult> SetVisibility(int id, [FromBody] SetVisibilityDto dto)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        int? sellerId = null;

        if (!isAdmin)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId)) return Forbid();

            sellerId = (await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId)).FirstOrDefault()?.Id;
            if (sellerId is null) return Forbid();
        }

        var result = await _service.SetCategoryHiddenAsync(id, dto.IsHidden, isAdmin, sellerId);

        return result switch
        {
            CategoryVisibilityResult.Ok => NoContent(),
            CategoryVisibilityResult.Forbidden => Forbid(),
            _ => NotFound("القسم غير موجود")
        };
    }
}