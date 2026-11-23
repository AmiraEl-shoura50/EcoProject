using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.Interfaces.Services;

public interface ICheckoutService
{
    // بترجّع null لو السلة فاضية أو طريقة الدفع غير صالحة أو في منتج مش متاح أو المخزون مش كافي
    Task<CheckoutResponseDto?> CheckoutAsync(int customerId, int paymentMethodId);
}