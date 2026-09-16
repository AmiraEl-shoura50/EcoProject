namespace ECommerce.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public Guid UserId { get; set; } // ✅ بنستخدم UserId مباشرة (مش CustomerId/SellerId) عشان تصلح للاتنين
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}