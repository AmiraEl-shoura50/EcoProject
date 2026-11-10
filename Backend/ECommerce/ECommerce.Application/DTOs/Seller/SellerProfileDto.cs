namespace ECommerce.Application.DTOs.Seller;

public class SellerProfileDto : SellerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}