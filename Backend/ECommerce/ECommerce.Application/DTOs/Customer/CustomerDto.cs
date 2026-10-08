namespace ECommerce.Application.DTOs.Customer;

public class CustomerDto
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}