namespace ECommerce.Domain.Entities;

public class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int OrderId { get; set; } // ✅ إثبات إنه اشترى المنتج فعليًا من خلال الأوردر ده
    public Order Order { get; set; } = null!;

    public int Rating { get; set; } // من 1 لـ 5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}