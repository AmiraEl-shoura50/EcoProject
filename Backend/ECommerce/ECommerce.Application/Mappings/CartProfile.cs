using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Items.Sum(i => i.Product.Price * i.Quantity)));

        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price));
    }
}