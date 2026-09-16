namespace ECommerce.Application.Settings;

public class PaymobSettings
{
    public string BaseUrl { get; set; } = "https://accept.paymob.com";
    public string SecretKey { get; set; } = string.Empty;   // ✅ بدل ApiKey
    public string PublicKey { get; set; } = string.Empty;   // ✅ جديد
    public string HmacSecret { get; set; } = string.Empty;
    public string NotificationUrl { get; set; } = string.Empty; // رابط الـ Webhook بتاعك (API/Payments/paymob/webhook)
    public string RedirectionUrl { get; set; } = string.Empty;  // فين العميل يرجع بعد الدفع (صفحة في الفرونت مثلاً)
}