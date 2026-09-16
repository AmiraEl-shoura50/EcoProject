using AutoMapper;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class WishlistProfile : Profile
{
    public WishlistProfile()
    {
        CreateMap<Wishlist, WishlistDto>();

        CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
    }
}