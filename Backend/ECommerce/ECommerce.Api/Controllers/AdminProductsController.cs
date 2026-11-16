using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = Roles.Admin)]
public class AdminProductsController : ControllerBase
{
    private readonly IProductVisibilityService _service;

    public AdminProductsController(IProductVisibilityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
    {
        queryParams.StockFilter = null;
        var result = await _service.GetAdminProductsAsync(queryParams);
        return Ok(result);
    }

    [HttpPut("{id:int}/visibility")]
    public async Task<IActionResult> SetVisibility(int id, [FromBody] SetVisibilityDto dto)
    {
        if (!await _service.SetProductHiddenByAdminAsync(id, dto.IsHidden)) return NotFound("المنتج غير موجود");
        return NoContent();
    }
}