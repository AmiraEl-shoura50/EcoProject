using System.Text.Json;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("paymob/webhook")]
    [AllowAnonymous] // ✅ Paymob نفسها اللي هتنادي الـ Endpoint ده، مش عميل مسجل دخول
    public async Task<IActionResult> PaymobWebhook([FromQuery] string hmac, [FromBody] JsonElement payload)
    {
        var processed = await _paymentService.ProcessWebhookAsync(payload, hmac);
        return processed ? Ok() : BadRequest();
    }
} 