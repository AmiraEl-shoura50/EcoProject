namespace ECommerce.Application.DTOs.Product;

public class AdminProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerStoreName { get; set; } = string.Empty;
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
    public bool IsCategoryHidden { get; set; }
}