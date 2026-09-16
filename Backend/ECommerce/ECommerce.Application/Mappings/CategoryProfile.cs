using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>()
             .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());
    }
}