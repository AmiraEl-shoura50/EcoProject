namespace ECommerce.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public float Rating { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public int SellerId { get; set; }
    public string SellerStoreName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}