using AutoMapper;
using ECommerce.Application.DTOs.Discount;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class DiscountProfile : Profile
{
    public DiscountProfile()
    {
        CreateMap<Discount, DiscountDto>()
            .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.DiscountType.ToString()))
            .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.DiscountTarget.Target.ToString()))
            .ForMember(dest => dest.TargetId, opt => opt.MapFrom(src => src.DiscountTarget.TargetId))
            .ForMember(dest => dest.TargetName, opt => opt.Ignore()); // ✅ هنملاها يدويًا في الـ Service (محتاجة Query إضافية)
    }
}