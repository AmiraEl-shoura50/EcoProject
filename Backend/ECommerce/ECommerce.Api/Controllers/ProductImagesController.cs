using System.Security.Claims;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/images")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _service;
    private readonly IUnitOfWork _unitOfWork;

    public ProductImagesController(IProductImageService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    // 6 صور × 5 ميجا ممكن تعدّي الحد الافتراضي (حوالي 30 ميجا) فبنرفعه لـ 40
    [HttpPut]
    [Authorize(Roles = Roles.Seller)]
    [RequestSizeLimit(40_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 40_000_000)]
    public async Task<IActionResult> Update(int productId, [FromForm] UpdateProductImagesDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId)) return Forbid();

        var seller = (await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId)).FirstOrDefault();
        if (seller is null) return Forbid();

        var images = await _service.UpdateAsync(productId, seller.Id, dto);
        if (images is null) return NotFound();

        return Ok(images);
    }
}