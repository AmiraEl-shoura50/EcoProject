using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/messages")]
[Authorize(Roles = Roles.Admin)]
public class AdminMessagesController : ControllerBase
{
    private readonly IContactService _contactService;

    public AdminMessagesController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filter = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _contactService.GetAllAsync(filter, search, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
        => await UnreadCountAsync();

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        if (!await _contactService.SetReadAsync(id, true)) return NotFound("الرسالة غير موجودة");
        return await UnreadCountAsync();
    }

    [HttpPut("{id:int}/unread")]
    public async Task<IActionResult> MarkUnread(int id)
    {
        if (!await _contactService.SetReadAsync(id, false)) return NotFound("الرسالة غير موجودة");
        return await UnreadCountAsync();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _contactService.MarkAllAsReadAsync();
        return await UnreadCountAsync();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _contactService.DeleteAsync(id)) return NotFound("الرسالة غير موجودة");
        return await UnreadCountAsync();
    }

    // كل عملية بترجّع العدد الجديد للغير مقروء، فالفرونت يحدّث العدّاد من غير طلب زيادة
    private async Task<IActionResult> UnreadCountAsync()
    {
        var count = await _contactService.GetUnreadCountAsync();
        return Ok(new { count });
    }
}