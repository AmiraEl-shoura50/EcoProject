using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = Roles.Admin)]
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentGroupService _service;

    public AdminPaymentsController(IPaymentGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filter = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetForAdminAsync(filter, search, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("awaiting-count")]
    public async Task<IActionResult> GetAwaitingCount()
        => await AwaitingCountAsync();

    [HttpPut("{id:int}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewPaymentProofDto dto)
    {
        var result = await _service.ReviewAsync(id, dto);

        return result switch
        {
            PaymentReviewResult.NotFound => NotFound("الدفعة غير موجودة"),
            PaymentReviewResult.InvalidState => BadRequest("لا يمكن مراجعة هذه الدفعة (ربما تمت مراجعتها بالفعل)"),
            _ => await AwaitingCountAsync()
        };
    }

    // كل عملية بترجّع العدد الجديد للمنتظر مراجعة، فالفرونت يحدّث العدّاد من غير طلب زيادة
    private async Task<IActionResult> AwaitingCountAsync()
    {
        var count = await _service.GetAwaitingCountAsync();
        return Ok(new { count });
    }
}