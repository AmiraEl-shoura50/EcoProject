namespace ECommerce.Application.DTOs.Wishlist;

public class WishlistItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}