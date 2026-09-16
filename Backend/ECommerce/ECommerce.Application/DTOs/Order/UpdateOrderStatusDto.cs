namespace ECommerce.Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    public string NewStatus { get; set; } = string.Empty; // "Confirmed", "Shipped", "Delivered"
}