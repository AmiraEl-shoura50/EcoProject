using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.PaymentMethod;

public class CreatePaymentMethodDto
{
    public string MethodName { get; set; } = string.Empty;
    public PaymentMethodType Type { get; set; }
    public string? ProviderIntegrationId { get; set; } // مطلوب بس لو Type = Gateway
}