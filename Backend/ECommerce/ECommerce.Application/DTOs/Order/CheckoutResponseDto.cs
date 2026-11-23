namespace ECommerce.Application.DTOs.Order;

public class CheckoutResponseDto
{
    public int? PaymentGroupId { get; set; }                     // موجود في التحويل اليدوي
    public decimal TotalAmount { get; set; }
    public List<CheckoutResultDto> Orders { get; set; } = new(); // بوابة الدفع: كل طلب ومعاه رابط الدفع
}