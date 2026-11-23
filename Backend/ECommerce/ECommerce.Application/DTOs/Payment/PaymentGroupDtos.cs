using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.DTOs.Payment;

public class PaymentGroupOrderDto
{
    public int OrderId { get; set; }
    public int SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class PaymentGroupDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty; // Pending | AwaitingConfirmation | Paid | Cancelled
    public decimal TotalAmount { get; set; }           // مجموع الطلبات غير الملغية = المطلوب تحويله
    public DateTime CreatedAt { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string? TransferUrl { get; set; }
    public bool CanUploadProof { get; set; }
    public string? LastRejectionReason { get; set; }
    public List<PaymentGroupOrderDto> Orders { get; set; } = new();
}

public class AdminPaymentGroupDto : PaymentGroupDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public List<PaymentProofDto> Proofs { get; set; } = new(); // الأحدث الأول
}

public class PendingPaymentDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
}