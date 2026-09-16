using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)] // ✅ Admin بس يقدر يوصل لأي حاجة هنا
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null)
    {
        var users = await _adminService.GetAllUsersAsync(role);
        return Ok(users);
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
        if (!updated) return NotFound();
        return NoContent();
    }
}