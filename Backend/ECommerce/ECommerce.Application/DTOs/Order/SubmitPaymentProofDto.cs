using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Order;

public class SubmitPaymentProofDto
{
    public IFormFile Image { get; set; } = null!;
    public string? TransferReference { get; set; }
}