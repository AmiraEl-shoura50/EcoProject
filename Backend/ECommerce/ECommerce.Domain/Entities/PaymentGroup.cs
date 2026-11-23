using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class PaymentGroup
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;

    public PaymentGroupStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PaymentProof> Proofs { get; set; } = new List<PaymentProof>();
}