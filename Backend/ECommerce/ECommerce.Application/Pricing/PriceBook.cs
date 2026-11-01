using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Pricing;

public record ProductPrice(decimal Original, decimal Final, int Percent, DateTime? EndDate)
{
    public bool HasDiscount => Final < Original;
}

// "دفتر أسعار" بيتحمّل مرة واحدة في الطلب، وبيحسب سعر أي منتج من الذاكرة
public class PriceBook
{
    private readonly IReadOnlyList<Discount> _discounts;
    private readonly IReadOnlyDictionary<int, int?> _categoryParents;

    public PriceBook(IReadOnlyList<Discount> discounts, IReadOnlyDictionary<int, int?> categoryParents)
    {
        _discounts = discounts;
        _categoryParents = categoryParents;
    }

    public ProductPrice GetPrice(Product product)
        => GetPrice(product.Id, product.CategoryId, product.SellerId, product.Price);

    public ProductPrice GetPrice(int productId, int categoryId, int sellerId, decimal price)
    {
        // القسم نفسه + القسم الرئيسي بتاعه (لو القسم فرعي)
        var categoryIds = new HashSet<int> { categoryId };
        if (_categoryParents.TryGetValue(categoryId, out var parentId) && parentId is not null)
            categoryIds.Add(parentId.Value);

        var bestPrice = price;
        Discount? best = null;

        foreach (var discount in _discounts)
        {
            // خصم بائع تاني (أو خصم قديم من غير صاحب) مبيمسش المنتج ده
            if (discount.SellerId != sellerId) continue;

            var target = discount.DiscountTarget;
            var applies = target.Target == TargetType.Product
                ? target.TargetId == productId
                : categoryIds.Contains(target.TargetId);
            if (!applies) continue;

            var discounted = Apply(discount, price);
            if (discounted is not null && discounted < bestPrice)
            {
                bestPrice = discounted.Value;
                best = discount;
            }
        }

        if (best is null)
            return new ProductPrice(price, price, 0, null);

        var percent = Math.Max(1, (int)Math.Round((1 - bestPrice / price) * 100));
        var endDate = DateTime.SpecifyKind(best.EndDate, DateTimeKind.Utc);

        return new ProductPrice(price, bestPrice, percent, endDate);
    }

    // بترجّع null لو الخصم مش منطقي (سعر صفر أو أعلى من الأصلي)
    private static decimal? Apply(Discount discount, decimal price)
    {
        var result = discount.DiscountType == DiscountType.Percentage
            ? price * (1 - discount.DiscountAmount / 100m)
            : price - discount.DiscountAmount;

        result = Math.Round(result, 2, MidpointRounding.AwayFromZero);

        return result > 0 && result < price ? result : null;
    }
    // البائعين اللي عندهم خصومات سارية (بنستخدمها عشان نحدد المنتجات المرشحة للعروض)
    public IReadOnlyCollection<int> SellerIds =>
        _discounts.Where(d => d.SellerId.HasValue)
                  .Select(d => d.SellerId!.Value)
                  .Distinct()
                  .ToList();
}