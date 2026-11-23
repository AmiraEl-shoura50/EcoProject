using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductImageService
{
    // بترجّع المعرض النهائي، أو null لو المنتج مش موجود أو مش بتاع البائع
    Task<List<ProductImageDto>?> UpdateAsync(int productId, int sellerId, UpdateProductImagesDto dto);
}