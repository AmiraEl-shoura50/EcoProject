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

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartDto> GetByCustomerIdAsync(int customerId)
    {
        var cart = await GetOrCreateCartAsync(customerId);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<bool> AddItemAsync(int customerId, AddToCartDto dto)
    {
        // ✅ نتأكد إن المنتج موجود ومتاح فعلاً قبل أي حاجة
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is null) return false;

        if (dto.Quantity <= 0 || dto.Quantity > product.StockQuantity) return false;

        var cart = await GetOrCreateCartAsync(customerId);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem is not null)
        {
            // ✅ لو المنتج موجود بالفعل في الكارت، بنزود الكمية بدل ما نضيف Row جديد
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

        cart.Items.Remove(item); // ✅ بما إن دي Composite Key، الحذف من الـ Collection كافي مع EF Core Tracking
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ✅ Helper - كل Customer المفروض يبقى ليه Cart تلقائي، فلو مش موجود بنعمله
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