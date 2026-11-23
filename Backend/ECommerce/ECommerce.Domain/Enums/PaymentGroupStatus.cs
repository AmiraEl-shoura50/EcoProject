namespace ECommerce.Domain.Enums;

public enum PaymentGroupStatus
{
    Pending = 0,               // في انتظار الدفع (أو الإثبات اترفض)
    AwaitingConfirmation = 1,  // العميل رفع إثبات وفي انتظار الأدمن
    Paid = 2                   // الأدمن أكد الدفع
}