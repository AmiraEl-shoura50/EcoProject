using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Application.Interfaces.Services;
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
    [AllowAnonymous] // أي حد يقدر يشوف طرق الدفع المتاحة
    public async Task<IActionResult> GetAll()
    {
        var methods = await _paymentMethodService.GetAllAsync();
        return Ok(methods);
    }

    [HttpPost]
    [Authorize] // أي مستخدم مسجل دخول - ممكن نحصرها بـ Admin بعدين لما نضيف Role كده
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodDto dto)
    {
        var created = await _paymentMethodService.CreateAsync(dto);
        return Ok(created);
    }
}