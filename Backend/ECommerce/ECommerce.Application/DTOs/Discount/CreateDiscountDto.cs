namespace ECommerce.Application.DTOs.Discount;

public class CreateDiscountDto
{
    public string DiscountType { get; set; } = string.Empty; // "Percentage" أو "FixedAmount"
    public decimal DiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TargetType { get; set; } = string.Empty; // "Product" أو "Category"
    public int TargetId { get; set; }
}