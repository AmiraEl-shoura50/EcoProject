using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class PaymentMethod
{
    public int Id { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public PaymentMethodType Type { get; set; }
    public string? ProviderIntegrationId { get; set; } // ✅ Integration ID بتاع Paymob - null لو Manual
    public string? TransferUrl { get; set; }
    public bool IsHidden { get; set; }

}