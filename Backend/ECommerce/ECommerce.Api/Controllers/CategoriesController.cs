using System.Security.Claims;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(ICategoryService categoryService, IUnitOfWork unitOfWork)
    {
        _categoryService = categoryService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParams queryParams)
    {
        var result = await _categoryService.SearchAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        var categories = await _categoryService.GetTopLevelWithChildrenAsync();
        return Ok(categories);
    }

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAdminTree()
    {
        var categories = await _categoryService.GetAdminTreeAsync();
        return Ok(categories);
    }

    // ✅ جديد: الأقسام الفرعية اللي أنشأها البائع الحالي
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetMine()
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var categories = await _categoryService.GetMineAsync(sellerId.Value);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Seller}")]
    public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        var userRole = isAdmin ? Roles.Admin : Roles.Seller;

        int? sellerId = null;
        if (!isAdmin)
        {
            sellerId = await GetCurrentSellerIdAsync();
            if (sellerId is null) return Forbid();
        }

        var created = await _categoryService.CreateAsync(dto, userRole, sellerId);

        if (created is null)
            return BadRequest("غير مسموح لك بإنشاء هذا النوع من الأقسام، أو القسم الرئيسي غير موجود");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Seller}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        var userRole = isAdmin ? Roles.Admin : Roles.Seller;

        int? sellerId = null;
        if (!isAdmin)
        {
            sellerId = await GetCurrentSellerIdAsync();
            if (sellerId is null) return Forbid();
        }

        var updated = await _categoryService.UpdateAsync(id, dto, userRole, sellerId);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Seller}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        var userRole = isAdmin ? Roles.Admin : Roles.Seller;

        int? sellerId = null;
        if (!isAdmin)
        {
            sellerId = await GetCurrentSellerIdAsync();
            if (sellerId is null) return Forbid();
        }

        var result = await _categoryService.DeleteAsync(id, userRole, sellerId);

        return result switch
        {
            DeleteCategoryResult.NotFound => NotFound(),
            DeleteCategoryResult.HasSubCategories => BadRequest("لا يمكن حذف قسم يحتوي على أقسام فرعية"),
            DeleteCategoryResult.HasProducts => BadRequest("لا يمكن حذف قسم يحتوي على منتجات. انقل المنتجات إلى قسم آخر أو احذفها أولًا"),
            _ => NoContent()
        };
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