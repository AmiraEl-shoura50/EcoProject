using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common;

public static class ProductVisibility
{
    // محتاج Category و Category.ParentCategory يكونوا محمّلين
    public static bool IsCategoryHidden(Category category)
        => category.IsHiddenBySeller || category.IsHiddenByAdmin ||
           (category.ParentCategory is { } parent && (parent.IsHiddenBySeller || parent.IsHiddenByAdmin));

    public static bool IsVisible(Product product)
        => !product.IsHiddenBySeller && !product.IsHiddenByAdmin && !IsCategoryHidden(product.Category);
}