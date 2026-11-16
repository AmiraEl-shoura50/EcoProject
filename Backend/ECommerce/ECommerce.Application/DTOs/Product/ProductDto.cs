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

    public int ReviewsCount { get; set; }
    public int SellerId { get; set; }
    public string SellerStoreName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    // ✅ جديد: بيتملوا بس لو في خصم ساري
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public DateTime? DiscountEndDate { get; set; }
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
    public bool IsCategoryHidden { get; set; }
}