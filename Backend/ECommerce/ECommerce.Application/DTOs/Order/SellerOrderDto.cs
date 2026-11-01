namespace ECommerce.Application.DTOs.Order;

public class SellerOrderDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;

    public List<OrderItemDto> Items { get; set; } = new();
    public PaymentProofDto? LatestPaymentProof { get; set; }
}