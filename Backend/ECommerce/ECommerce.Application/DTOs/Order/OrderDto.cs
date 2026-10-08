namespace ECommerce.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public string PaymentMethodType { get; set; } = string.Empty;

}