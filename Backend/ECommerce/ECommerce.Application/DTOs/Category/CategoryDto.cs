namespace ECommerce.Application.DTOs.Category;


public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public int? ParentCategoryId { get; set; }
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}