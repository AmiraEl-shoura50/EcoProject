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
    private readonly IDashboardService _dashboardService;

    public AdminController(IAdminService adminService, IDashboardService dashboardService)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int tzOffset = 0)
    {
        var dashboard = await _dashboardService.GetAdminAsync(Math.Clamp(tzOffset, -840, 840));
        return Ok(dashboard);
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