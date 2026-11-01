namespace ECommerce.Application.DTOs.PaymentMethod;

public class UpdatePaymentMethodDto
{
    public string MethodName { get; set; } = string.Empty;
    public string? ProviderIntegrationId { get; set; }
    public string? TransferUrl { get; set; }
}