namespace ECommerce.Application.DTOs.Seller;

public class SellerDto
{
    public int Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int ProductsCount { get; set; }
}