using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var methods = await _paymentMethodService.GetAllAsync();
        return Ok(methods);
    }

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAllForAdmin()
    {
        var methods = await _paymentMethodService.GetAllForAdminAsync();
        return Ok(methods);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodDto dto)
    {
        var created = await _paymentMethodService.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}/visibility")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetVisibility(int id, [FromBody] SetPaymentMethodVisibilityDto dto)
    {
        var updated = await _paymentMethodService.SetVisibilityAsync(id, dto.IsHidden);
        if (!updated) return NotFound("طريقة الدفع غير موجودة");
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentMethodDto dto)
    {
        var updated = await _paymentMethodService.UpdateAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _paymentMethodService.DeleteAsync(id);

        return result switch
        {
            DeletePaymentMethodResult.NotFound => NotFound(),
            DeletePaymentMethodResult.HasOrders => BadRequest("لا يمكن حذف طريقة دفع مرتبطة بطلبات سابقة"),
            _ => NoContent()
        };
    }
}