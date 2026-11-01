using System.Security.Claims;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _notificationService.GetMyNotificationsAsync(GetCurrentUserId(), pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(GetCurrentUserId());
        return Ok(new { count });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(GetCurrentUserId());
        return NoContent();
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var updated = await _notificationService.MarkAsReadAsync(id, GetCurrentUserId());
        if (!updated) return NotFound();
        return NoContent();
    }

    private Guid GetCurrentUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteRead()
    {
        await _notificationService.DeleteReadAsync(GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _notificationService.DeleteAsync(id, GetCurrentUserId());
        if (!deleted) return NotFound();
        return NoContent();
    }
}