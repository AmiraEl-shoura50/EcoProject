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

    public async Task<IEnumerable<DiscountDto>> GetMineAsync(int sellerId)
    {
        var discounts = await _unitOfWork.Discounts.GetBySellerAsync(sellerId);
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

    public async Task<DiscountDto> CreateAsync(CreateDiscountDto dto, int sellerId)
    {
        // الرسائل هنا بتوصل للمستخدم كما هي (InvalidOperationException بيتحول لـ 400)
        if (dto.StartDate >= dto.EndDate)
            throw new InvalidOperationException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

        if (dto.EndDate <= DateTime.UtcNow)
            throw new InvalidOperationException("تاريخ انتهاء الخصم يجب أن يكون في المستقبل");

        if (!Enum.TryParse<TargetType>(dto.TargetType, true, out var targetType) || !Enum.IsDefined(targetType))
            throw new InvalidOperationException("نوع الهدف غير صحيح");

        if (!Enum.TryParse<DiscountType>(dto.DiscountType, true, out var discountType) || !Enum.IsDefined(discountType))
            throw new InvalidOperationException("نوع الخصم غير صحيح");

        string? productName = null;

        if (targetType == TargetType.Product)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.TargetId);
            if (product is null || product.SellerId != sellerId)
                throw new InvalidOperationException("المنتج غير موجود أو لا يخصك");

            if (discountType == DiscountType.FixedAmount && dto.DiscountAmount >= product.Price)
                throw new InvalidOperationException("قيمة الخصم يجب أن تكون أقل من سعر المنتج");

            productName = product.Name;
        }
        else
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.TargetId);
            if (category is null)
                throw new InvalidOperationException("القسم غير موجود");
            // الخصم على قسم بيمس منتجات البائع نفسه داخل القسم ده بس (الربط بـ SellerId)
        }

        // ✅ نمنع خصمين متداخلين زمنيًا على نفس الهدف لنفس البائع
        var existing = await _unitOfWork.Discounts.GetBySellerAsync(sellerId);
        var overlaps = existing.Any(d =>
            d.DiscountTarget.Target == targetType &&
            d.DiscountTarget.TargetId == dto.TargetId &&
            dto.StartDate < d.EndDate &&
            dto.EndDate > d.StartDate);

        if (overlaps)
            throw new InvalidOperationException("يوجد خصم آخر على نفس الهدف في هذه الفترة");

        // ✅ حفظ واحد للهدف والخصم معًا: يا الاتنين يتحفظوا يا مفيش حاجة تتحفظ
        var discount = new Discount
        {
            SellerId = sellerId,
            DiscountType = discountType,
            DiscountAmount = dto.DiscountAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DiscountTarget = new DiscountTarget
            {
                Target = targetType,
                TargetId = dto.TargetId
            }
        };

        await _unitOfWork.Discounts.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Discounts.GetByIdWithTargetAsync(discount.Id);
        var resultDto = _mapper.Map<DiscountDto>(created);
        await FillTargetNamesAsync(new List<DiscountDto> { resultDto });

        // ✅ إشعار العملاء اللي حاطين المنتج ده في المفضلة
        if (targetType == TargetType.Product)
        {
            var wishlistItems = await _unitOfWork.Wishlists.GetCustomersWhoWishlistedProductAsync(dto.TargetId);

            foreach (var item in wishlistItems)
            {
                await _notificationService.CreateAndSendAsync(
                    item.Wishlist.Customer.UserId,
                    "عرض جديد! 🎉",
                    $"في خصم جديد على {productName} الموجود في قائمة المفضلة");
            }
        }

        return resultDto;
    }

    public async Task<bool> DeleteAsync(int id, int sellerId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdWithTargetAsync(id);

        // ✅ الملكية بالـ SellerId لكل الأنواع (منتج وقسم)
        if (discount is null || discount.SellerId != sellerId) return false;

        _unitOfWork.Discounts.Delete(discount);
        _unitOfWork.DiscountTargets.Delete(discount.DiscountTarget);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsForProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null) return Enumerable.Empty<DiscountDto>();

        var discounts = await _unitOfWork.Discounts.GetActiveForProductAsync(productId, product.CategoryId);

        // ✅ خصومات صاحب المنتج بس
        var ownDiscounts = discounts.Where(d => d.SellerId == product.SellerId);

        var dtos = _mapper.Map<List<DiscountDto>>(ownDiscounts);
        await FillTargetNamesAsync(dtos);
        return dtos;
    }

    // TargetName مش FK حقيقي، فمحتاجين Query يدوي لكل نوع
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