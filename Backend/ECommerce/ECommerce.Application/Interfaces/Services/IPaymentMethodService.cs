using ECommerce.Application.DTOs.PaymentMethod;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetAllAsync();
    Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto);
}