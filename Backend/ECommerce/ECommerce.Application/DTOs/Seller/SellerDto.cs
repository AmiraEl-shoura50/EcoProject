namespace ECommerce.Application.DTOs.Seller;

public class SellerDto
{
    public int Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public float Rating { get; set; }
    public string? ImageUrl { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}