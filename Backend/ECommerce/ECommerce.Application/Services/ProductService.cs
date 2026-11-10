using AutoMapper;
using ECommerce.Application.Constants;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Pricing;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPricingService _pricingService;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService fileStorageService,
        IPricingService pricingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _pricingService = pricingService;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var dtos = _mapper.Map<List<ProductDto>>(products);
        await ApplyDiscountsAsync(dtos);
        return dtos;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
        if (product is null) return null;

        var dto = _mapper.Map<ProductDto>(product);
        await ApplyDiscountsAsync(new[] { dto });
        return dto;
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
        var dtos = _mapper.Map<List<ProductDto>>(products);
        await ApplyDiscountsAsync(dtos);
        return dtos;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId)
    {
        var product = _mapper.Map<Product>(dto);
        product.SellerId = sellerId;

        if (dto.Image is not null)
        {
            using var stream = dto.Image.OpenReadStream();
            product.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "products");
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
        var result = _mapper.Map<ProductDto>(created);
        await ApplyDiscountsAsync(new[] { result });
        return result;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto, int sellerId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null) return false;

        if (product.SellerId != sellerId) return false;

        if (dto.Image is not null)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var oldPublicId = _fileStorageService.ExtractPublicId(product.ImageUrl);
                await _fileStorageService.DeleteImageAsync(oldPublicId);
            }

            using var stream = dto.Image.OpenReadStream();
            product.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "products");
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeleteProductResult> DeleteAsync(int id, int sellerId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null || product.SellerId != sellerId) return DeleteProductResult.NotFound;

        var hasOrders = (await _unitOfWork.Orders.FindAsync(o => o.Items.Any(i => i.ProductId == id))).Any();
        if (hasOrders) return DeleteProductResult.HasOrders;

        var cartItems = await _unitOfWork.CartItems.FindAsync(ci => ci.ProductId == id);
        foreach (var item in cartItems) _unitOfWork.CartItems.Delete(item);

        var wishlistItems = await _unitOfWork.WishlistItems.FindAsync(wi => wi.ProductId == id);
        foreach (var item in wishlistItems) _unitOfWork.WishlistItems.Delete(item);

        var imageUrl = product.ImageUrl;

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            var publicId = _fileStorageService.ExtractPublicId(imageUrl);
            await _fileStorageService.DeleteImageAsync(publicId);
        }

        return DeleteProductResult.Deleted;
    }

    public async Task<DeleteProductsResultDto> DeleteManyAsync(List<int> productIds, int sellerId)
    {
        var result = new DeleteProductsResultDto();

        foreach (var id in productIds.Distinct())
        {
            switch (await DeleteAsync(id, sellerId))
            {
                case DeleteProductResult.Deleted:
                    result.DeletedCount++;
                    break;
                case DeleteProductResult.HasOrders:
                    result.BlockedCount++;
                    break;
            }
        }

        return result;
    }

    public async Task<PaginatedResultDto<ProductDto>> SearchAsync(ProductQueryParams queryParams)
    {
        var (items, totalCount) = await _unitOfWork.Products.SearchAsync(queryParams);
        var dtos = _mapper.Map<List<ProductDto>>(items);
        await ApplyDiscountsAsync(dtos);

        return new PaginatedResultDto<ProductDto>
        {
            Items = dtos,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };
    }

    // بيحسب الخصم لكل المنتجات في استعلامين بس (الخصومات السارية والأقسام)
   
    private async Task ApplyDiscountsAsync(IEnumerable<ProductDto> dtos)
    {
        var list = dtos.ToList();
        if (list.Count == 0) return;

        var book = await _pricingService.LoadAsync();
        ApplyDiscounts(list, book);
    }

    private static void ApplyDiscounts(IEnumerable<ProductDto> dtos, PriceBook book)
    {
        foreach (var dto in dtos)
        {
            var price = book.GetPrice(dto.Id, dto.CategoryId, dto.SellerId, dto.Price);
            if (!price.HasDiscount) continue;

            dto.DiscountedPrice = price.Final;
            dto.DiscountPercent = price.Percent;
            dto.DiscountEndDate = price.EndDate;
        }
    }
    // المنتجات اللي عليها خصم ساري فعلًا، بالترتيب والترقيم على الأسعار بعد الخصم
    public async Task<PaginatedResultDto<ProductDto>> GetOffersAsync(ProductQueryParams queryParams)
    {
        var book = await _pricingService.LoadAsync();

        var empty = new PaginatedResultDto<ProductDto>
        {
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = 0
        };

        var sellerIds = book.SellerIds;
        if (sellerIds.Count == 0) return empty;

        var infos = await _unitOfWork.Products.GetPricingInfoAsync(sellerIds);

        // نحسب السعر لكل منتج مرشح، ونبقي بس اللي ليه خصم فعلي
        // (خصم مبلغ ثابت أكبر من السعر مثلًا بيتجاهله دفتر الأسعار)
        var deals = infos
            .Select(i => (Info: i, Price: book.GetPrice(i.Id, i.CategoryId, i.SellerId, i.Price)))
            .Where(x => x.Price.HasDiscount);

        var sorted = queryParams.SortBy switch
        {
            "price_asc" => deals.OrderBy(x => x.Price.Final).ThenByDescending(x => x.Info.Id),
            "price_desc" => deals.OrderByDescending(x => x.Price.Final).ThenByDescending(x => x.Info.Id),
            "rating" => deals.OrderByDescending(x => x.Info.Rating).ThenByDescending(x => x.Info.Id),
            "newest" => deals.OrderByDescending(x => x.Info.Id).ThenByDescending(x => x.Info.Id),
            _ => deals.OrderByDescending(x => x.Price.Percent).ThenByDescending(x => x.Info.Id) // الأعلى خصمًا (الافتراضي)
        };

        var ordered = sorted.ToList();
        if (ordered.Count == 0) return empty;

        var pageIds = ordered
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(x => x.Info.Id)
            .ToList();

        // نجيب بس منتجات الصفحة الحالية، وبنحافظ على الترتيب اللي حسبناه
        var products = await _unitOfWork.Products.GetByIdsWithDetailsAsync(pageIds);
        var byId = products.ToDictionary(p => p.Id);

        var dtos = pageIds
            .Where(byId.ContainsKey)
            .Select(id => _mapper.Map<ProductDto>(byId[id]))
            .ToList();

        ApplyDiscounts(dtos, book);

        return new PaginatedResultDto<ProductDto>
        {
            Items = dtos,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = ordered.Count
        };
    }
    public async Task<StockSummaryDto> GetStockSummaryAsync(int sellerId)
    {
        var (outOfStock, lowStock) = await _unitOfWork.Products
            .GetStockSummaryForSellerAsync(sellerId, StockRules.LowStockThreshold);

        return new StockSummaryDto { OutOfStock = outOfStock, LowStock = lowStock };
    }
}