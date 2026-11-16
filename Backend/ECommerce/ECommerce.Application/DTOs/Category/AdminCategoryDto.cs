namespace ECommerce.Application.DTOs.Category;

public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ProductsCount { get; set; }
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
    public List<AdminSubCategoryDto> SubCategories { get; set; } = new();
}

public class AdminSubCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductsCount { get; set; }
    public string? StoreName { get; set; }
    public bool IsHiddenBySeller { get; set; }
    public bool IsHiddenByAdmin { get; set; }
}