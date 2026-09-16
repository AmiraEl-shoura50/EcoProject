namespace ECommerce.Application.Interfaces.Services;

public class PaymobBillingData
{
    public string Email { get; set; } = "amiraelshoura41@gmail.com";
    public string FirstName { get; set; } = "N/A";
    public string LastName { get; set; } = "N/A";
    public string Phone { get; set; } = "+201011342692";
}

public interface IPaymentGatewayService
{
    Task<(string ProviderOrderId, string IframeUrl)> InitiatePaymentAsync(
        int merchantOrderId, decimal amount, string integrationId, PaymobBillingData billingData);

    bool VerifyWebhookHmac(Dictionary<string, string> orderedFields, string receivedHmac);
}