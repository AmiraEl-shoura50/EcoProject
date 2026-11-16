using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Repositories;

public static class VisibilityExtensions
{
    // المنتج ظاهر للعملاء لو: مش مخفي (من صاحبه أو الإدارة)، وقسمه (وقسمه الرئيسي لو فرعي) مش مخفيين
    public static IQueryable<Product> Visible(this IQueryable<Product> query)
        => query.Where(p =>
            !p.IsHiddenBySeller && !p.IsHiddenByAdmin &&
            !p.Category.IsHiddenBySeller && !p.Category.IsHiddenByAdmin &&
            (p.Category.ParentCategory == null ||
             (!p.Category.ParentCategory.IsHiddenBySeller && !p.Category.ParentCategory.IsHiddenByAdmin)));

    // عكس Visible
    public static IQueryable<Product> Hidden(this IQueryable<Product> query)
        => query.Where(p =>
            p.IsHiddenBySeller || p.IsHiddenByAdmin ||
            p.Category.IsHiddenBySeller || p.Category.IsHiddenByAdmin ||
            (p.Category.ParentCategory != null &&
             (p.Category.ParentCategory.IsHiddenBySeller || p.Category.ParentCategory.IsHiddenByAdmin)));

    public static IQueryable<Category> Visible(this IQueryable<Category> query)
        => query.Where(c =>
            !c.IsHiddenBySeller && !c.IsHiddenByAdmin &&
            (c.ParentCategory == null ||
             (!c.ParentCategory.IsHiddenBySeller && !c.ParentCategory.IsHiddenByAdmin)));
}