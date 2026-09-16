namespace ECommerce.Domain.Enums;

public enum OrderStatus
{
    Pending,               // اتعمل - مستني دفع (يدوي أو بوابة)
    AwaitingConfirmation,  // (Manual) إثبات مرفوع مستني مراجعة، أو (Gateway) اتحول لصفحة الدفع مستني الـ Webhook
    Confirmed,             // الدفع اتأكد
    Shipped,
    Delivered,
    Cancelled
}