using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper, IPricingService pricingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _pricingService = pricingService;
    }

    public async Task<CartDto> GetByCustomerIdAsync(int customerId)
    {
        var cart = await GetOrCreateCartAsync(customerId);
        var dto = _mapper.Map<CartDto>(cart);

        if (dto.Items.Count > 0)
        {
            var book = await _pricingService.LoadAsync();

            foreach (var item in dto.Items)
            {
                var product = cart.Items.First(i => i.ProductId == item.ProductId).Product;
                var price = book.GetPrice(product);

                item.OriginalUnitPrice = product.Price;
                item.UnitPrice = price.Final;
            }

            dto.TotalPrice = dto.Items.Sum(i => i.Subtotal);
        }

        return dto;
    }

    public async Task<bool> AddItemAsync(int customerId, AddToCartDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is null) return false;

        if (dto.Quantity <= 0 || dto.Quantity > product.StockQuantity) return false;

        var cart = await GetOrCreateCartAsync(customerId);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + dto.Quantity;
            if (newQuantity > product.StockQuantity) return false;

            existingItem.Quantity = newQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateItemAsync(int customerId, int productId, UpdateCartItemDto dto)
    {
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is null) return false;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null || dto.Quantity <= 0 || dto.Quantity > product.StockQuantity) return false;

        item.Quantity = dto.Quantity;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int customerId, int productId)
    {
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is null) return false;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        cart.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task<Cart> GetOrCreateCartAsync(int customerId)
    {
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is not null) return cart;

        cart = new Cart { CustomerId = customerId };
        await _unitOfWork.Carts.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return cart;
    }
}