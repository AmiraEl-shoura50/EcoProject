namespace ECommerce.Application.DTOs.Cart;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    public decimal OriginalUnitPrice { get; set; } // ✅ جديد: السعر قبل الخصم
    public decimal UnitPrice { get; set; }         // السعر بعد الخصم (اللي بيتحاسب بيه)

    public int Quantity { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}