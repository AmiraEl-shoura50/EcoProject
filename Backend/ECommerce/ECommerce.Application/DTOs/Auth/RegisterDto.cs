namespace ECommerce.Application.DTOs.Auth;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // "Customer" أو "Seller"

    // Customer-specific
    public string? Address { get; set; }

    // Seller-specific
    public string? StoreName { get; set; }
}