using ECommerce.Application.DTOs.PaymentMethod;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetAllAsync();
    Task<IEnumerable<AdminPaymentMethodDto>> GetAllForAdminAsync();
    Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto);
    Task<bool> UpdateAsync(int id, UpdatePaymentMethodDto dto);
    Task<DeletePaymentMethodResult> DeleteAsync(int id);
    Task<bool> SetVisibilityAsync(int id, bool isHidden);
}