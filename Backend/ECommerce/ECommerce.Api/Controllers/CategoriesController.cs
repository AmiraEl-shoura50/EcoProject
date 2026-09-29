using ECommerce.Application.DTOs.Category;
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

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    //[HttpGet]
    //[AllowAnonymous] // أي حد يقدر يشوف الـ Categories من غير Login
    //public async Task<IActionResult> GetAll()
    //{
    //    var categories = await _categoryService.GetAllAsync();
    //    return Ok(categories);
    //}
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParams queryParams)
    {
        var result = await _categoryService.SearchAsync(queryParams);
        return Ok(result);
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
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Seller;
        var created = await _categoryService.CreateAsync(dto, userRole);

        if (created is null)
            return BadRequest("غير مسموح لك بإنشاء هذا النوع من الأقسام، أو القسم الرئيسي غير موجود");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        var categories = await _categoryService.GetTopLevelWithChildrenAsync();
        return Ok(categories);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto)
    {
        var updated = await _categoryService.UpdateAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}