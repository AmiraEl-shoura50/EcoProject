using System.Security.Claims;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/payment-groups")]
[Authorize(Roles = Roles.Customer)]
public class PaymentGroupsController : ControllerBase
{
    private readonly IPaymentGroupService _service;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentGroupsController(IPaymentGroupService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        return Ok(await _service.GetPendingForCustomerAsync(customerId.Value));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var group = await _service.GetForCustomerAsync(id, customerId.Value);
        if (group is null) return NotFound();

        return Ok(group);
    }

    [HttpPost("{id:int}/proof")]
    public async Task<IActionResult> SubmitProof(int id, [FromForm] SubmitPaymentProofDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var proof = await _service.SubmitProofAsync(id, customerId.Value, dto);
        if (proof is null) return BadRequest("تعذر رفع إثبات الدفع - تأكد من حالة الدفعة");

        return Ok(proof);
    }

    private async Task<int?> GetCurrentCustomerIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var customers = await _unitOfWork.Customers.FindAsync(c => c.UserId == userId);
        return customers.FirstOrDefault()?.Id;
    }
}