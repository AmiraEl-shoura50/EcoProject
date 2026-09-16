using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Category;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}