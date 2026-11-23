using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Product;

public class UpdateProductImagesDto
{
    // الترتيب النهائي للمعرض: "e:12" = صورة موجودة رقمها 12، و"n:0" = أول ملف في NewFiles
    // أي صورة موجودة مش مذكورة هنا بتتحذف، والأولى في الترتيب هي الرئيسية
    public List<string> Order { get; set; } = new();
    public List<IFormFile> NewFiles { get; set; } = new();
}