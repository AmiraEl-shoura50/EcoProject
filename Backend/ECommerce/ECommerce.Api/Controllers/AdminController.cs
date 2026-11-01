using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetUsersAsync(role, search, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserActiveDto dto)
    {
        var updated = await _adminService.SetUserActiveStatusAsync(id, dto.IsActive);
        if (!updated) return NotFound("المستخدم غير موجود");
        return NoContent();
    }
}