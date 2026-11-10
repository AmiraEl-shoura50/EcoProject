using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Validators.Common;

public static class ImageRules
{
    public const long MaxBytes = 5 * 1024 * 1024; // 5MB

    // الصورة اختيارية: لو موجودة لازم تكون من نوع صورة وحجمها مقبول
    public static IRuleBuilderOptions<T, IFormFile?> MustBeValidImage<T>(this IRuleBuilder<T, IFormFile?> rule)
        => rule
            .Must(file => file is null || file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("الملف يجب أن يكون صورة")
            .Must(file => file is null || file.Length <= MaxBytes)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت");
}