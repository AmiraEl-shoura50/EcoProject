using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class ProductImageService : IProductImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ProductImageService> _logger;

    public ProductImageService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<ProductImageService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<List<ProductImageDto>?> UpdateAsync(int productId, int sellerId, UpdateProductImagesDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null || product.SellerId != sellerId) return null;

        var existing = await _unitOfWork.ProductImages.GetByProductAsync(productId);
        var existingById = existing.ToDictionary(i => i.Id);

        // خطة الترتيب النهائي (الـ Validator اتأكد إن الشكل صحيح)
        var plan = new List<(bool IsNew, int Key)>();

        foreach (var token in dto.Order)
        {
            var isNew = token[0] == 'n';
            var key = int.Parse(token[2..]);

            if (!isNew && !existingById.ContainsKey(key))
                throw new InvalidOperationException("إحدى الصور غير موجودة، حدّث الصفحة وحاول مرة أخرى");

            plan.Add((isNew, key));
        }

        // ✅ نرفع الجديدة الأول: لو أي رفع فشل، منعدّل في الداتابيز حاجة ونمسح اللي اترفع
        var uploaded = new Dictionary<int, string>();

        try
        {
            foreach (var (_, key) in plan.Where(p => p.IsNew))
            {
                var file = dto.NewFiles[key];
                using var stream = file.OpenReadStream();
                uploaded[key] = await _fileStorageService.UploadImageAsync(stream, file.FileName, "products");
            }

            var keepIds = plan.Where(p => !p.IsNew).Select(p => p.Key).ToHashSet();
            var removed = existing.Where(i => !keepIds.Contains(i.Id)).ToList();

            foreach (var image in removed)
                _unitOfWork.ProductImages.Delete(image);

            var finalList = new List<ProductImage>();
            var order = 0;

            foreach (var (isNew, key) in plan)
            {
                if (isNew)
                {
                    var image = new ProductImage { ProductId = productId, ImageUrl = uploaded[key], SortOrder = order++ };
                    await _unitOfWork.ProductImages.AddAsync(image);
                    finalList.Add(image);
                }
                else
                {
                    var image = existingById[key];
                    image.SortOrder = order++;
                    _unitOfWork.ProductImages.Update(image);
                    finalList.Add(image);
                }
            }

            // الأولى في الترتيب هي الصورة الرئيسية (بتظهر في الكروت والسلة والطلبات)
            product.ImageUrl = finalList.FirstOrDefault()?.ImageUrl;
            _unitOfWork.Products.Update(product);

            await _unitOfWork.SaveChangesAsync();

            // الحفظ نجح: نمسح الصور المحذوفة من Cloudinary (بأقصى جهد)
            foreach (var image in removed)
                await TryDeleteFromStorageAsync(image.ImageUrl);

            return finalList.Select(i => new ProductImageDto { Id = i.Id, Url = i.ImageUrl }).ToList();
        }
        catch
        {
            foreach (var url in uploaded.Values)
                await TryDeleteFromStorageAsync(url);

            throw;
        }
    }

    private async Task TryDeleteFromStorageAsync(string url)
    {
        try
        {
            await _fileStorageService.DeleteImageAsync(_fileStorageService.ExtractPublicId(url));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر مسح الصورة من التخزين: {Url}", url);
        }
    }
}