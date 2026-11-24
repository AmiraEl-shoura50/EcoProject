namespace ECommerce.Domain.Entities;

public class Settlement
{
    public int Id { get; set; }

    public int SellerId { get; set; }
    public Seller Seller { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime CutoffDay { get; set; }          // اليوم المحدد (بتوقيت الأدمن) اللي اتحسب لحد نهايته

    public int OrdersCount { get; set; }
    public decimal GrossAmount { get; set; }          // إجمالي الطلبات المتوصّلة
    public decimal CommissionPercent { get; set; }    // نسبة العمولة وقت التسوية (بتتسجّل ومبتتغيّرش)
    public decimal CommissionAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }     // تعديل يدوي: موجب = إضافة، سالب = خصم
    public string? AdjustmentNote { get; set; }
    public decimal NetAmount { get; set; }            // الصافي المحوَّل = الإجمالي - العمولة + التعديل

    public string PayoutMethod { get; set; } = string.Empty;   // بيانات التحويل وقت التسوية
    public string PayoutAccount { get; set; } = string.Empty;
    public string? Reference { get; set; }            // رقم عملية التحويل

    public Guid CreatedByUserId { get; set; }         // الأدمن اللي سجّل التحويل

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}