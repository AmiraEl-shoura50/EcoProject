using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Seller;

public class UpdateSellerDto
{
    public string StoreName { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}