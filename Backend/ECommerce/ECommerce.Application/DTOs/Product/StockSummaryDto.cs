namespace ECommerce.Application.DTOs.Product;

public class StockSummaryDto
{
    public int OutOfStock { get; set; }
    public int LowStock { get; set; }
    public int HiddenCount { get; set; }
}