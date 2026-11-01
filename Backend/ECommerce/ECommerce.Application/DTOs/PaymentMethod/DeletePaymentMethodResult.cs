namespace ECommerce.Application.DTOs.PaymentMethod;

public enum DeletePaymentMethodResult
{
    Deleted,
    NotFound,
    HasOrders
}