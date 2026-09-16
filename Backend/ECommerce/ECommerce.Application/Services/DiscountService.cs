using AutoMapper;
using ECommerce.Application.DTOs.Discount;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    public DiscountService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<DiscountDto>> GetAllAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetAllWithTargetAsync();
        var dtos = _mapper.Map<List<DiscountDto>>(discounts);

        await FillTargetNamesAsync(dtos);
        return dtos;
    }

    public async Task<DiscountDto?> GetByIdAsync(int id)
    {
        var discount = await _unitOfWork.Discounts.GetByIdWithTargetAsync(id);
        if (discount is null) return null;

        var dto = _mapper.Map<DiscountDto>(discount);
        await FillTargetNamesAsync(new List<DiscountDto> { dto });
        return dto;
    }

    public async Task<DiscountDto?> CreateAsync(CreateDiscountDto dto, int sellerId)
    {
        // ✅ Validation أساسي على التواريخ
        if (dto.StartDate >= dto.EndDate) return null;

        if (!Enum.TryParse<TargetType>(dto.TargetType, true, out var targetType)) return null;
        if (!Enum.TryParse<DiscountType>(dto.DiscountType, true, out var discountType)) return null;

        // ✅✅ الأهم - نتحقق يدويًا إن الـ TargetId فعلاً موجود، وإنه بتاع الـ Seller ده
        if (targetType == TargetType.Product)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.TargetId);
            if (product is null || product.SellerId != sellerId) return null;
        }
        else if (targetType == TargetType.Category)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.TargetId);
            if (category is null) return null;
            // ملحوظة: الـ Category مش مرتبطة بـ Seller معين في التصميم الحالي، فأي Seller يقدر يحط خصم على قسم
        }

        var discountTarget = new DiscountTarget
        {
            Target = targetType,
            TargetId = dto.TargetId
        };

        await _unitOfWork.DiscountTargets.AddAsync(discountTarget);
        await _unitOfWork.SaveChangesAsync(); // ✅ لازم نحفظ الأول عشان ناخد الـ Id بتاعه

        var discount = new Discount
        {
            DiscountTargetId = discountTarget.Id,
            DiscountType = discountType,
            DiscountAmount = dto.DiscountAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _unitOfWork.Discounts.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Discounts.GetByIdWithTargetAsync(discount.Id);
        var resultDto = _mapper.Map<DiscountDto>(created);
        await FillTargetNamesAsync(new List<DiscountDto> { resultDto });

        // ✅ إشعار العملاء اللي حاطين المنتج ده في الـ Wishlist
        if (targetType == TargetType.Product)
        {
            var wishlistItems = await _unitOfWork.Wishlists.GetCustomersWhoWishlistedProductAsync(dto.TargetId);

            foreach (var item in wishlistItems)
            {
                await _notificationService.CreateAndSendAsync(
                    item.Wishlist.Customer.UserId,
                    "عرض جديد! 🎉",
                    $"في خصم جديد على منتج في قائمة أمنياتك");
            }
        }

        return resultDto;
    }

    public async Task<bool> DeleteAsync(int id, int sellerId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdWithTargetAsync(id);
        if (discount is null) return false;

        // ✅ تأكيد الملكية قبل الحذف
        if (discount.DiscountTarget.Target == TargetType.Product)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(discount.DiscountTarget.TargetId);
            if (product is null || product.SellerId != sellerId) return false;
        }

        _unitOfWork.Discounts.Delete(discount);
        _unitOfWork.DiscountTargets.Delete(discount.DiscountTarget); // ✅ بنمسح الـ Target كمان عشان منسبش بيانات يتيمة
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsForProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null) return Enumerable.Empty<DiscountDto>();

        var discounts = await _unitOfWork.Discounts.GetActiveForProductAsync(productId, product.CategoryId);
        var dtos = _mapper.Map<List<DiscountDto>>(discounts);

        await FillTargetNamesAsync(dtos);
        return dtos;
    }

    // ✅ Helper - بيملى TargetName لأنها مش FK حقيقي، فمحتاجين Query يدوي لكل نوع
    private async Task FillTargetNamesAsync(List<DiscountDto> dtos)
    {
        foreach (var dto in dtos)
        {
            if (dto.TargetType == TargetType.Product.ToString())
            {
                var product = await _unitOfWork.Products.GetByIdAsync(dto.TargetId);
                dto.TargetName = product?.Name ?? "منتج محذوف";
            }
            else if (dto.TargetType == TargetType.Category.ToString())
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.TargetId);
                dto.TargetName = category?.Name ?? "قسم محذوف";
            }
        }
    }
}