namespace ECommerce.Application.DTOs.Product;

public class UpdateStockDto
{
    public string Mode { get; set; } = "add"; // "add" إضافة | "set" تحديد
    public int Quantity { get; set; }
}