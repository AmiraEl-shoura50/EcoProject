using AutoMapper;
using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class PaymentMethodProfile : Profile
{
    public PaymentMethodProfile()
    {
        CreateMap<PaymentMethod, PaymentMethodDto>();
        CreateMap<CreatePaymentMethodDto, PaymentMethod>();
    }
}