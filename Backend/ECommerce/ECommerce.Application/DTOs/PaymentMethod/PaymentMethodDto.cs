namespace ECommerce.Application.DTOs.PaymentMethod;

public class PaymentMethodDto
{
    public int Id { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}