namespace ECommerce.Application.DTOs.Discount;

public class DiscountDto
{
    public int Id { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TargetType { get; set; } = string.Empty; // "Product" أو "Category"
    public int TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty; // اسم المنتج أو القسم
}