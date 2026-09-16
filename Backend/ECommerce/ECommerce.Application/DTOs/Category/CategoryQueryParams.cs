namespace ECommerce.Application.DTOs.Category;

public class CategoryQueryParams
{
    public string? SearchTerm { get; set; }

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
        set => _pageSize = value switch
        {
            < 1 => 10,
            > 50 => 50,
            _ => value
        };
    }
}