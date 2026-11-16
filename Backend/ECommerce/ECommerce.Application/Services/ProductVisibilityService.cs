using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class ProductVisibilityService : IProductVisibilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProductVisibilityService> _logger;

    public ProductVisibilityService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ProductVisibilityService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    // قائمة منتجات الأدمن: كل المنتجات بما فيها المخفية
    public async Task<PaginatedResultDto<AdminProductDto>> GetAdminProductsAsync(ProductQueryParams queryParams)
    {
        queryParams.IncludeHidden = true;

        var (items, totalCount) = await _unitOfWork.Products.SearchAsync(queryParams);

        return new PaginatedResultDto<AdminProductDto>
        {
            Items = items.Select(p => new AdminProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                SellerId = p.SellerId,
                SellerStoreName = p.Seller.StoreName,
                IsHiddenBySeller = p.IsHiddenBySeller,
                IsHiddenByAdmin = p.IsHiddenByAdmin,
                IsCategoryHidden = ProductVisibility.IsCategoryHidden(p.Category)
            }).ToList(),
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> SetProductHiddenBySellerAsync(int productId, int sellerId, bool isHidden)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null || product.SellerId != sellerId) return false;

        product.IsHiddenBySeller = isHidden;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetProductHiddenByAdminAsync(int productId, bool isHidden)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null) return false;
        if (product.IsHiddenByAdmin == isHidden) return true;

        product.IsHiddenByAdmin = isHidden;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        await NotifySellerAsync(
            product.SellerId,
            isHidden ? "تم إخفاء أحد منتجاتك" : "تم إظهار منتجك من جديد",
            isHidden
                ? $"تم إخفاء منتجك «{product.Name}» عن العملاء بواسطة الإدارة، ولا يمكنك إظهاره"
                : $"تم إظهار منتجك «{product.Name}» للعملاء من جديد بواسطة الإدارة",
            "/seller/products");

        return true;
    }

    public async Task<CategoryVisibilityResult> SetCategoryHiddenAsync(
        int categoryId, bool isHidden, bool isAdmin, int? sellerId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        if (category is null) return CategoryVisibilityResult.NotFound;

        if (isAdmin)
        {
            if (category.IsHiddenByAdmin == isHidden) return CategoryVisibilityResult.Ok;
            category.IsHiddenByAdmin = isHidden;
        }
        else
        {
            // البائع بيتحكم في أقسامه الفرعية اللي هو عملها بس
            if (sellerId is null || category.CreatedBySellerId != sellerId) return CategoryVisibilityResult.Forbidden;
            category.IsHiddenBySeller = isHidden;
        }

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        // لو الأدمن أخفى قسم فرعي عمله بائع: نبلّغه
        if (isAdmin && category.CreatedBySellerId is { } owner)
        {
            await NotifySellerAsync(
                owner,
                isHidden ? "تم إخفاء أحد أقسامك" : "تم إظهار قسمك من جديد",
                isHidden
                    ? $"تم إخفاء القسم «{category.Name}» ومنتجاته عن العملاء بواسطة الإدارة"
                    : $"تم إظهار القسم «{category.Name}» للعملاء من جديد بواسطة الإدارة",
                "/seller/categories");
        }

        return CategoryVisibilityResult.Ok;
    }

    // بأقصى جهد: التغيير اتحفظ، وفشل الإشعار ميكسرش العملية
    private async Task NotifySellerAsync(int sellerId, string title, string message, string link)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller is null) return;

            await _notificationService.CreateAndSendAsync(seller.UserId, title, message, link);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر إرسال إشعار الإخفاء للبائع {SellerId}", sellerId);
        }
    }
}