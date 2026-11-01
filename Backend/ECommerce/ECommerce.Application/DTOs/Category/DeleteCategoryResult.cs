namespace ECommerce.Application.DTOs.Category;

public enum DeleteCategoryResult
{
    Deleted,
    NotFound,
    HasProducts,
    HasSubCategories
}