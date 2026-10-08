namespace ECommerce.Application.DTOs.Order;

public class DeleteOrdersDto
{
    public List<int> OrderIds { get; set; } = new();
}