using System.Security.Claims;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/orders")]

public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IOrderService orderService, IUnitOfWork unitOfWork)
    {
        _orderService = orderService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("checkout")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var results = await _orderService.CheckoutAsync(customerId.Value, dto);
        if (!results.Any())
            return BadRequest("تعذر إتمام الطلب - تأكدي من الكارت وطريقة الدفع والمخزون المتاح");

        return Ok(results);
    }

    [HttpPost("{id}/payment-proof")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> SubmitPaymentProof(int id, [FromForm] SubmitPaymentProofDto dto)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var proof = await _orderService.SubmitPaymentProofAsync(customerId.Value, id, dto);
        if (proof is null) return BadRequest("تعذر رفع إثبات الدفع - تأكدي من حالة الطلب وطريقة الدفع");

        return Ok(proof);
    }
    [HttpPut("{id}/review-payment")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> ReviewPayment(int id, [FromBody] ReviewPaymentProofDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var reviewed = await _orderService.ReviewPaymentProofAsync(sellerId.Value, id, dto);
        if (!reviewed) return BadRequest("تعذر مراجعة إثبات الدفع");

        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var orders = await _orderService.GetMyOrdersAsync(customerId.Value, pageNumber, pageSize);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetById(int id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var order = await _orderService.GetByIdAsync(id, customerId.Value);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Cancel(int id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId is null) return Forbid();

        var cancelled = await _orderService.CancelAsync(id, customerId.Value);
        if (!cancelled) return BadRequest("لا يمكن إلغاء هذا الطلب");
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

    [HttpGet("seller/mine")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetSellerOrders()
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var orders = await _orderService.GetOrdersForSellerAsync(sellerId.Value);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var updated = await _orderService.UpdateStatusAsync(id, sellerId.Value, dto.NewStatus);
        if (!updated) return BadRequest("لا يمكن تحديث حالة الطلب - تأكدي من صحة الانتقال المطلوب");

        return NoContent();
    }

    // ✅ Helper إضافي بما إن الـ Controller ده فيه Customer و Seller مع بعض دلوقتي
    private async Task<int?> GetCurrentSellerIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var sellers = await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId);
        return sellers.FirstOrDefault()?.Id;
    }
}