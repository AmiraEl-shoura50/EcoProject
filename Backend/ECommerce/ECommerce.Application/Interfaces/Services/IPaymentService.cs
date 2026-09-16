using System.Text.Json;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<string?> InitiateGatewayPaymentAsync(int orderId);
    Task<bool> ProcessWebhookAsync(JsonElement payload, string receivedHmac);
}