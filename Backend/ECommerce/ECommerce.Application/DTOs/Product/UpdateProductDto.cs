using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Product;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; } // ✅ اختياري - لو null، الصورة القديمة تفضل زي ما هي
}