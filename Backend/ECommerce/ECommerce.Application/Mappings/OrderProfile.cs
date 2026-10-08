using AutoMapper;
using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.MethodName))
            .ForMember(dest => dest.PaymentMethodType, opt => opt.MapFrom(src => src.PaymentMethod.Type.ToString()));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

        CreateMap<PaymentProof, PaymentProofDto>();

       
    }
}