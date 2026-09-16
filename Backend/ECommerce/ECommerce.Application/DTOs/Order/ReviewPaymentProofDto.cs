namespace ECommerce.Application.DTOs.Order;

public class ReviewPaymentProofDto
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}