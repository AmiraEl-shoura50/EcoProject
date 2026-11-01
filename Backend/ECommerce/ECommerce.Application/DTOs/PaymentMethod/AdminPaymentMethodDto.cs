namespace ECommerce.Application.DTOs.PaymentMethod;

public class AdminPaymentMethodDto
{
    public int Id { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ProviderIntegrationId { get; set; }
    public string? TransferUrl { get; set; }
    public bool IsHidden { get; set; }
    public int OrdersCount { get; set; }
}