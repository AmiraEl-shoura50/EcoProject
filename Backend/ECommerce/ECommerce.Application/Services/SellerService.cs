using ECommerce.Application.DTOs.Seller;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Application.Services;

public class SellerService : ISellerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public SellerService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    // عام: من غير أي بيانات شخصية
    public async Task<SellerDto?> GetByIdAsync(int id)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(id);
        if (seller is null) return null;

        var (average, reviewsCount, productsCount) = await GetStatsAsync(id);

        return new SellerDto
        {
            Id = seller.Id,
            StoreName = seller.StoreName,
            ImageUrl = seller.ImageUrl,
            Rating = average,
            ReviewsCount = reviewsCount,
            ProductsCount = productsCount
        };
    }

    // خاص بالبائع نفسه
    public async Task<SellerProfileDto?> GetMineAsync(int sellerId)
    {
        var seller = await _unitOfWork.Sellers.GetWithUserAsync(sellerId);
        if (seller is null) return null;

        var (average, reviewsCount, productsCount) = await GetStatsAsync(sellerId);

        return new SellerProfileDto
        {
            Id = seller.Id,
            StoreName = seller.StoreName,
            ImageUrl = seller.ImageUrl,
            Rating = average,
            ReviewsCount = reviewsCount,
            ProductsCount = productsCount,
            FirstName = seller.User.FirstName,
            LastName = seller.User.LastName,
            Email = seller.User.Email ?? string.Empty
        };
    }

    public async Task<bool> UpdateAsync(int sellerId, UpdateSellerDto dto)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
        if (seller is null) return false;

        var name = dto.StoreName.Trim();

        // منع تكرار اسم المتجر (بدون حساسية لحالة الأحرف)
        var duplicates = await _unitOfWork.Sellers.FindAsync(s =>
            s.Id != sellerId && s.StoreName.ToLower() == name.ToLower());

        if (duplicates.Any())
            throw new InvalidOperationException("اسم المتجر مستخدم بالفعل، اختر اسمًا آخر");

        string? oldImageUrl = null;

        if (dto.Image is not null)
        {
            // نرفع الجديدة الأول، فلو الرفع فشل الصورة القديمة تفضل زي ما هي
            using var stream = dto.Image.OpenReadStream();
            var newUrl = await _fileStorageService.UploadImageAsync(stream, dto.Image.FileName, "sellers");

            oldImageUrl = seller.ImageUrl;
            seller.ImageUrl = newUrl;
        }
        else if (dto.RemoveImage && !string.IsNullOrEmpty(seller.ImageUrl))
        {
            oldImageUrl = seller.ImageUrl;
            seller.ImageUrl = null;
        }

        seller.StoreName = name;

        _unitOfWork.Sellers.Update(seller);
        await _unitOfWork.SaveChangesAsync();

        // نمسح الصورة القديمة بعد نجاح الحفظ مش قبله
        if (!string.IsNullOrEmpty(oldImageUrl))
        {
            var publicId = _fileStorageService.ExtractPublicId(oldImageUrl);
            await _fileStorageService.DeleteImageAsync(publicId);
        }

        return true;
    }

    private async Task<(double Average, int ReviewsCount, int ProductsCount)> GetStatsAsync(int sellerId)
    {
        var (average, reviewsCount) = await _unitOfWork.Reviews.GetSellerStatsAsync(sellerId);
        var productsCount = await _unitOfWork.Products.CountBySellerAsync(sellerId);

        return (average, reviewsCount, productsCount);
    }
}