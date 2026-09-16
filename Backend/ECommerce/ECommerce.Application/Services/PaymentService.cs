using System.Text.Json;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _gateway;
    private readonly INotificationService _notificationService;

    //private readonly ILogger<PaymentService> _logger = LoggerFactory.Create(builder =>
    //{
    //    builder.AddConsole();
    //}).CreateLogger<PaymentService>();

    public PaymentService(IUnitOfWork unitOfWork, IPaymentGatewayService gateway, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _gateway = gateway;
        _notificationService = notificationService;
    }

    public async Task<string?> InitiateGatewayPaymentAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null || order.PaymentMethod.Type != PaymentMethodType.Gateway) return null;
        if (string.IsNullOrEmpty(order.PaymentMethod.ProviderIntegrationId)) return null;

        var billing = new PaymobBillingData
        {
            Email = order.Customer.User.Email ?? "customer@example.com",
            FirstName = order.Customer.User.FirstName,
            LastName = order.Customer.User.LastName,
            Phone = order.Customer.User.PhoneNumber ?? "01000000000"
        };

        var (providerOrderId, iframeUrl) = await _gateway.InitiatePaymentAsync(
            order.Id, order.TotalAmount, order.PaymentMethod.ProviderIntegrationId, billing);

        var transaction = new PaymentTransaction
        {
            OrderId = order.Id,
            ProviderOrderId = providerOrderId,
            Amount = order.TotalAmount,
            Status = PaymentTransactionStatus.Initiated,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PaymentTransactions.AddAsync(transaction);

        order.Status = OrderStatus.AwaitingConfirmation;
        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        return iframeUrl;
    }

    public async Task<bool> ProcessWebhookAsync(JsonElement payload, string receivedHmac)
    {
        if (!payload.TryGetProperty("obj", out var obj)) return false;

        var orderedFields = new Dictionary<string, string>
        {
            ["amount_cents"] = obj.GetProperty("amount_cents").ToString(),
            ["created_at"] = obj.GetProperty("created_at").GetString() ?? "",
            ["currency"] = obj.GetProperty("currency").GetString() ?? "",
            ["error_occured"] = obj.GetProperty("error_occured").GetBoolean().ToString().ToLower(),
            ["has_parent_transaction"] = obj.GetProperty("has_parent_transaction").GetBoolean().ToString().ToLower(),
            ["id"] = obj.GetProperty("id").ToString(),
            ["integration_id"] = obj.GetProperty("integration_id").ToString(),
            ["is_3d_secure"] = obj.GetProperty("is_3d_secure").GetBoolean().ToString().ToLower(),
            ["is_auth"] = obj.GetProperty("is_auth").GetBoolean().ToString().ToLower(),
            ["is_capture"] = obj.GetProperty("is_capture").GetBoolean().ToString().ToLower(),
            ["is_refunded"] = obj.GetProperty("is_refunded").GetBoolean().ToString().ToLower(),
            ["is_standalone_payment"] = obj.GetProperty("is_standalone_payment").GetBoolean().ToString().ToLower(),
            ["is_voided"] = obj.GetProperty("is_voided").GetBoolean().ToString().ToLower(),
            ["order_id"] = obj.GetProperty("order").GetProperty("id").ToString(),
            ["owner"] = obj.GetProperty("owner").ToString(),
            ["pending"] = obj.GetProperty("pending").GetBoolean().ToString().ToLower(),
            ["source_data_pan"] = obj.GetProperty("source_data").GetProperty("pan").GetString() ?? "",
            ["source_data_sub_type"] = obj.GetProperty("source_data").GetProperty("sub_type").GetString() ?? "",
            ["source_data_type"] = obj.GetProperty("source_data").GetProperty("type").GetString() ?? "",
            ["success"] = obj.GetProperty("success").GetBoolean().ToString().ToLower()
        };
    //    _logger.LogInformation("Paymob Webhook Payload: {Payload}", payload.GetRawText());

        if (!_gateway.VerifyWebhookHmac(orderedFields, receivedHmac))
            return false;

        // ✅✅ التغيير الأهم - special_reference هي اللي بتحمل رقم الأوردر بتاعنا دلوقتي
        var specialReference = obj.GetProperty("order").TryGetProperty("merchant_order_id", out var moid)
            ? moid.GetString()
            : payload.TryGetProperty("special_reference", out var sr) ? sr.GetString() : null;

        if (specialReference is null || !int.TryParse(specialReference, out var orderId))
            return false;

        var isSuccess = obj.GetProperty("success").GetBoolean();
        var transactionId = obj.GetProperty("id").ToString();

        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null) return false;

        var transaction = order.PaymentTransactions
            .Where(t => t.Status == PaymentTransactionStatus.Initiated)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault();

        if (transaction is null) return false;

        transaction.ProviderTransactionId = transactionId;

        if (isSuccess)
        {
            transaction.Status = PaymentTransactionStatus.Paid;
            transaction.PaidAt = DateTime.UtcNow;
            order.Status = OrderStatus.Confirmed;

            await _notificationService.CreateAndSendAsync(
                order.Customer.UserId, "تم تأكيد الدفع ✅", $"تم تأكيد دفعتك للطلب #{order.Id} بنجاح");
        }
        else
        {
            transaction.Status = PaymentTransactionStatus.Failed;
            order.Status = OrderStatus.Pending;

            await _notificationService.CreateAndSendAsync(
                order.Customer.UserId, "فشلت عملية الدفع ❌", $"لم تتم عملية الدفع للطلب #{order.Id}، برجاء إعادة المحاولة");
        }

        _unitOfWork.PaymentTransactions.Update(transaction);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}