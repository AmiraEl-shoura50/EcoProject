namespace ECommerce.Application.DTOs.Settlement;

public class ReportOrderRow
{
    public int OrderId { get; set; }
    public DateTime CreatedLocal { get; set; }
    public DateTime DeliveredLocal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public decimal Net { get; set; }
}

public class ReportItemRow
{
    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class SettlementReportData
{
    public int SettlementId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public string PayoutMethod { get; set; } = string.Empty;
    public string PayoutAccount { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public DateTime CreatedLocal { get; set; }
    public DateTime CutoffDay { get; set; }
    public int OrdersCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public string? AdjustmentNote { get; set; }
    public decimal NetAmount { get; set; }
    public List<ReportOrderRow> Orders { get; set; } = new();
    public List<ReportItemRow> Items { get; set; } = new();
}

public class PeriodSellerRow
{
    public string StoreName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public string PayoutMethod { get; set; } = string.Empty;
    public string PayoutAccount { get; set; } = string.Empty;
    public decimal? CommissionPercent { get; set; }
    public int OrdersCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal UnsettledNet { get; set; }   // الصافي اللي لسه ما اتحوّلش
    public string Note { get; set; } = string.Empty;
}

public class PeriodOrderRow
{
    public int OrderId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime CreatedLocal { get; set; }
    public DateTime DeliveredLocal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal Amount { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal Commission { get; set; }
    public decimal Net { get; set; }
    public string Status { get; set; } = string.Empty;   // "تمت تسويته #12" أو "غير مسوّى"
}

public class PeriodReportData
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<PeriodSellerRow> Sellers { get; set; } = new();
    public List<PeriodOrderRow> Orders { get; set; } = new();
}