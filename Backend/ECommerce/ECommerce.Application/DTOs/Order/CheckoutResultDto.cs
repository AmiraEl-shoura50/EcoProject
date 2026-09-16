namespace ECommerce.Application.DTOs.Order;

public class CheckoutResultDto
{
    public OrderDto Order { get; set; } = null!;
    public string? PaymentUrl { get; set; } // ✅ موجودة بس لو PaymentMethod.Type = Gateway
}