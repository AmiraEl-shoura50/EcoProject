using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public IFormFile? Image { get; set; }
    public int CategoryId { get; set; }
    // ❌ SellerId اتشالت من هنا خالص
}