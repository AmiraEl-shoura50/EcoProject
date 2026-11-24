using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Seller;

public class UpdateSellerDto
{
    public string StoreName { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public bool RemoveImage { get; set; }
    public string? PayoutMethod { get; set; }    // "VodafoneCash" | "InstaPay" | "BankAccount" أو فاضي
    public string? PayoutAccount { get; set; }
}