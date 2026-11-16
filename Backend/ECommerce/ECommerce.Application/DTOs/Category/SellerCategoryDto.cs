namespace ECommerce.Application.DTOs.Category;

public class SellerCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string ParentCategoryName { get; set; } = string.Empty;
    public int ProductsCount { get; set; }
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
}