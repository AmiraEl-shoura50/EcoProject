namespace ECommerce.Application.DTOs.Product;

public class ProductQueryParams
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // "price_asc", "price_desc", "rating", "newest"

    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        // ✅ نحدد أقصى حجم صفحة عشان محدش يطلب 100,000 صف مرة واحدة
        set => _pageSize = value switch
        {
            < 1 => 10,
            > 50 => 50,
            _ => value
        };
    }
}