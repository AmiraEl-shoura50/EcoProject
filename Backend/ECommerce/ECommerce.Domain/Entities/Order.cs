using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal TotalAmount { get; set; }

    public bool IsDeletedByCustomer { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<PaymentProof> PaymentProofs { get; set; } = new List<PaymentProof>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>(); // ✅ جديد
}