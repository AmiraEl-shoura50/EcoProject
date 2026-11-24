using System.Security.Claims;
using ECommerce.Application.DTOs.Settlement;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/settlements")]
[Authorize(Roles = Roles.Admin)]
public class AdminSettlementsController : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly ISettlementService _service;

    public AdminSettlementsController(ISettlementService service)
    {
        _service = service;
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances([FromQuery] string cutoff, [FromQuery] int tzOffset = 0)
        => Ok(await _service.GetBalancesAsync(cutoff, tzOffset));

    [HttpPut("sellers/{sellerId:int}/commission")]
    public async Task<IActionResult> SetCommission(int sellerId, [FromBody] SetCommissionDto dto)
    {
        if (!await _service.SetCommissionAsync(sellerId, dto.CommissionPercent)) return NotFound("البائع غير موجود");
        return NoContent();
    }

    [HttpPost("sellers/{sellerId:int}/settle")]
    public async Task<IActionResult> Settle(int sellerId, [FromBody] CreateSettlementDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var settlement = await _service.CreateAsync(sellerId, dto, adminId);

        return Ok(settlement);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? sellerId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _service.GetHistoryAsync(sellerId, pageNumber, pageSize));

    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> ExportSettlement(int id, [FromQuery] int tzOffset = 0)
    {
        var bytes = await _service.ExportSettlementAsync(id, null, tzOffset);
        if (bytes is null) return NotFound("التسوية غير موجودة");

        return File(bytes, ExcelContentType, $"settlement-{id}.xlsx");
    }

    [HttpGet("report/export")]
    public async Task<IActionResult> ExportPeriodReport(
        [FromQuery] string from, [FromQuery] string to, [FromQuery] int tzOffset = 0)
    {
        var bytes = await _service.ExportPeriodReportAsync(from, to, tzOffset);
        return File(bytes, ExcelContentType, $"report-{from}_{to}.xlsx");
    }
}