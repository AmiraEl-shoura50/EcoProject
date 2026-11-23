using AutoMapper;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SellerStoreName, opt => opt.MapFrom(src => src.Seller.StoreName))
            .ForMember(dest => dest.IsCategoryHidden, opt => opt.MapFrom(src =>
                src.Category.IsHiddenBySeller || src.Category.IsHiddenByAdmin ||
                (src.Category.ParentCategory != null &&
                 (src.Category.ParentCategory.IsHiddenBySeller || src.Category.ParentCategory.IsHiddenByAdmin))));



        CreateMap<CreateProductDto, Product>()
             .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // بنحطها يدوي في الـ Service

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

        CreateMap<ProductImage, ProductImageDto>()
           .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ImageUrl));


    }
}