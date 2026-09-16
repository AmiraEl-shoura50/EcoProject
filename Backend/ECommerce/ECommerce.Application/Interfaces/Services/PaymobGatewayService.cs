using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Settings;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services;

public class PaymobGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly PaymobSettings _settings;

    public PaymobGatewayService(HttpClient httpClient, IOptions<PaymobSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<(string ProviderOrderId, string IframeUrl)> InitiatePaymentAsync(
        int merchantOrderId, decimal amount, string integrationId, PaymobBillingData billingData)
    {
        var amountCents = (int)(amount * 100);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v1/intention/");
        request.Headers.Add("Authorization", $"Token {_settings.SecretKey}"); // ✅ بدل Bearer Token القديم

        request.Content = JsonContent.Create(new
        {
            amount = amountCents,
            currency = "EGP",
            payment_methods = new[] { int.Parse(integrationId) },
            special_reference = merchantOrderId.ToString(), // ✅ بنستخدمها بدل merchant_order_id عشان نلاقي الأوردر لما الـ Webhook يوصل
            notification_url = _settings.NotificationUrl,
            redirection_url = _settings.RedirectionUrl,
            billing_data = new
            {
                email = billingData.Email,
                first_name = billingData.FirstName,
                last_name = billingData.LastName,
                phone_number = billingData.Phone,
                apartment = "NA",
                floor = "NA",
                street = "NA",
                building = "NA",
                shipping_method = "NA",
                postal_code = "NA",
                city = "NA",
                country = "EG",
                state = "NA"
            }
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var intentionId = json.GetProperty("id").ToString();
        var clientSecret = json.GetProperty("client_secret").GetString()!;

        // ✅ رابط الـ Unified Checkout بدل الـ Iframe القديم
        var checkoutUrl = $"{_settings.BaseUrl}/unifiedcheckout/?publicKey={_settings.PublicKey}&clientSecret={clientSecret}";

        return (intentionId, checkoutUrl);
    }

    // ✅ نفس منطق الـ HMAC القديم لسه شغال - Paymob بتستخدم نفس آلية الـ Callback للـ Transaction
    public bool VerifyWebhookHmac(Dictionary<string, string> orderedFields, string receivedHmac)
    {
        var concatenated = string.Concat(orderedFields.Values);

        var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
        var messageBytes = Encoding.UTF8.GetBytes(concatenated);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        var computedHmac = Convert.ToHexString(hashBytes).ToLower();

        return computedHmac == receivedHmac.ToLower();
    }
}