using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId)
    {
        var product = _mapper.Map<Product>(dto);
        product.SellerId = sellerId;

        // ✅ رفع الصورة لو موجودة
        if (dto.Image is not null)
        {
            using var stream = dto.Image.OpenReadStream();
            product.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "products");
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto, int sellerId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return false;

        if (product.SellerId != sellerId) return false;

        // ✅ التعامل مع الصورة
        if (dto.Image is not null)
        {
            // نمسح الصورة القديمة لو موجودة
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var oldPublicId = _fileStorageService.ExtractPublicId(product.ImageUrl);
                await _fileStorageService.DeleteImageAsync(oldPublicId);
            }

            // نرفع الجديدة
            using var stream = dto.Image.OpenReadStream();
            product.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "products");
        }
        // لو dto.Image فاضية، مش هنلمس product.ImageUrl خالص - تفضل زي ما هي

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, int sellerId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return false;

        if (product.SellerId != sellerId) return false;

        // ✅ نمسح الصورة من Cloudinary الأول
        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            var publicId = _fileStorageService.ExtractPublicId(product.ImageUrl);
            await _fileStorageService.DeleteImageAsync(publicId);
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<PaginatedResultDto<ProductDto>> SearchAsync(ProductQueryParams queryParams)
    {
        var (items, totalCount) = await _unitOfWork.Products.SearchAsync(queryParams);

        return new PaginatedResultDto<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };
    }
}