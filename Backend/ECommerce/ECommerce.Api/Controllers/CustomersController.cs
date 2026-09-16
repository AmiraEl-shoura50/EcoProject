using System.Security.Claims;
using ECommerce.Application.DTOs.Customer;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IUnitOfWork _unitOfWork;

    public CustomersController(ICustomerService customerService, IUnitOfWork unitOfWork)
    {
        _customerService = customerService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetMe()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var customer = await _customerService.GetByIdAsync(customerId.Value);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpPut("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateCustomerDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var updated = await _customerService.UpdateAsync(customerId.Value, dto);
        if (!updated) return NotFound();
        return NoContent();
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