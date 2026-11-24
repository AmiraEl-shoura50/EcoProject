namespace ECommerce.Application.DTOs.Settlement;

// صف داخلي: طلب متوصّل لسه ما اتسوّاش
public record UnsettledOrderRow(int OrderId, int SellerId, decimal Amount, DateTime DeliveredAt);

public record PaidTotal(decimal Total, int Count);

public class SellerBalanceDto
{
    public int SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }

    public decimal? CommissionPercent { get; set; }
    public string? PayoutMethod { get; set; }
    public string? PayoutAccount { get; set; }

    public int OrdersCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }

    public decimal TotalPaid { get; set; }
    public int SettlementsCount { get; set; }

    public string? BlockReason { get; set; } // لو مش null: السبب اللي بيمنع التسوية دلوقتي
}

public class SettlementDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime CutoffDay { get; set; }
    public int OrdersCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public string? AdjustmentNote { get; set; }
    public decimal NetAmount { get; set; }
    public string PayoutMethod { get; set; } = string.Empty;
    public string PayoutAccount { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class CreateSettlementDto
{
    public string Cutoff { get; set; } = string.Empty;   // yyyy-MM-dd (يوم بتوقيت الأدمن)
    public int TzOffset { get; set; }                     // فرق توقيت المتصفح بالدقايق
    public int ExpectedOrdersCount { get; set; }          // عدد الطلبات اللي الأدمن شافها في المعاينة
    public decimal AdjustmentAmount { get; set; }
    public string? AdjustmentNote { get; set; }
    public string? Reference { get; set; }
}

public class SetCommissionDto
{
    public decimal CommissionPercent { get; set; }
}

public class SellerEarningsDto
{
    public decimal? CommissionPercent { get; set; }
    public string? PayoutMethod { get; set; }
    public string? PayoutAccount { get; set; }

    public int UnsettledOrdersCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }      // المستحق دلوقتي (قبل أي تعديل يدوي)

    public decimal TotalPaid { get; set; }
    public int SettlementsCount { get; set; }
}