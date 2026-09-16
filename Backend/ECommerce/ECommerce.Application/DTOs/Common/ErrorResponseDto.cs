namespace ECommerce.Application.DTOs.Common;

public class ErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; } // بتتملى بس في الـ Development
    public int StatusCode { get; set; }
}