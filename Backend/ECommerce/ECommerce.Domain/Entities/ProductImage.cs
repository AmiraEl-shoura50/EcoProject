namespace ECommerce.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }   // 0 = الصورة الرئيسية
}