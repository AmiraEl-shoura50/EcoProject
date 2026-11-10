namespace ECommerce.Application.Constants;

public static class StockRules
{
    // أقل من أو يساوي الرقم ده = "قاربت على النفاد" (نفس حد "متبقي X فقط" في الموقع)
    public const int LowStockThreshold = 5;
}