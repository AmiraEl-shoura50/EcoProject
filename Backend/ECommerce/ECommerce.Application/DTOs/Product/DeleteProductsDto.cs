namespace ECommerce.Application.DTOs.Product;

public class DeleteProductsDto
{
    public List<int> ProductIds { get; set; } = new();
}