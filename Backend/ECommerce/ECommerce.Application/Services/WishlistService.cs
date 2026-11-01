using AutoMapper;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;

    public WishlistService(IUnitOfWork unitOfWork, IMapper mapper, IPricingService pricingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _pricingService = pricingService;
    }

    public async Task<WishlistDto> GetByCustomerIdAsync(int customerId)
    {
        var wishlist = await GetOrCreateWishlistAsync(customerId);
        var dto = _mapper.Map<WishlistDto>(wishlist);

        if (dto.Items.Count > 0)
        {
            var book = await _pricingService.LoadAsync();

            foreach (var item in dto.Items)
            {
                var product = wishlist.Items.First(i => i.ProductId == item.ProductId).Product;
                var price = book.GetPrice(product);

                item.OriginalPrice = product.Price;
                item.Price = price.Final;
            }
        }

        return dto;
    }

    public async Task<bool> AddItemAsync(int customerId, AddToWishlistDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is null) return false;

        var wishlist = await GetOrCreateWishlistAsync(customerId);

        var alreadyExists = wishlist.Items.Any(i => i.ProductId == dto.ProductId);
        if (alreadyExists) return true;

        wishlist.Items.Add(new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId = dto.ProductId
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int customerId, int productId)
    {
        var wishlist = await _unitOfWork.Wishlists.GetByCustomerIdAsync(customerId);
        if (wishlist is null) return false;

        var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        wishlist.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task<Wishlist> GetOrCreateWishlistAsync(int customerId)
    {
        var wishlist = await _unitOfWork.Wishlists.GetByCustomerIdAsync(customerId);
        if (wishlist is not null) return wishlist;

        wishlist = new Wishlist { CustomerId = customerId };
        await _unitOfWork.Wishlists.AddAsync(wishlist);
        await _unitOfWork.SaveChangesAsync();

        return wishlist;
    }
}