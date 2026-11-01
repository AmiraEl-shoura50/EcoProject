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
             .ForMember(dest => dest.PaymentMethodType, opt => opt.MapFrom(src => src.PaymentMethod.Type.ToString()))
             .ForMember(dest => dest.PaymentTransferUrl, opt => opt.MapFrom(src => src.PaymentMethod.TransferUrl));

        CreateMap<OrderItem, OrderItemDto>()
             .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
             .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
             .ForMember(dest => dest.IsReviewed, opt => opt.Ignore());

        CreateMap<PaymentProof, PaymentProofDto>();

        CreateMap<Order, SellerOrderDto>()
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
             .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.MethodName))
             .ForMember(dest => dest.PaymentMethodType, opt => opt.MapFrom(src => src.PaymentMethod.Type.ToString()))
             .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.User.FirstName + " " + src.Customer.User.LastName))
             .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.User.PhoneNumber))
             .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.Customer.Address))
             .ForMember(dest => dest.LatestPaymentProof, opt => opt.MapFrom(src => src.PaymentProofs.OrderByDescending(p => p.SubmittedAt).FirstOrDefault()));
    }
}