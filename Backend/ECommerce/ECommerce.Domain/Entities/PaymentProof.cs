namespace ECommerce.Domain.Entities;

public class PaymentProof
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public string? TransferReference { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool? IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}