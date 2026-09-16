using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string ProviderOrderId { get; set; } = string.Empty; // رقم الأوردر عند Paymob
    public string? ProviderTransactionId { get; set; } // رقم العملية بعد الدفع

    public decimal Amount { get; set; }
    public PaymentTransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}