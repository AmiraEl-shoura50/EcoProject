namespace ECommerce.Application.DTOs.Wishlist;

public class WishlistItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal OriginalPrice { get; set; } // ✅ جديد
    public decimal Price { get; set; }         // بعد الخصم
    public int StockQuantity { get; set; }
    public float Rating { get; set; }
}