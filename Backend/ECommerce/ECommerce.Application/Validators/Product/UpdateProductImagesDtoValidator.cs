using System.Text.RegularExpressions;
using ECommerce.Application.Constants;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Validators.Common;
using FluentValidation;

namespace ECommerce.Application.Validators.Product;

public class UpdateProductImagesDtoValidator : AbstractValidator<UpdateProductImagesDto>
{
    private static readonly Regex TokenPattern = new(@"^(e|n):\d{1,9}$", RegexOptions.Compiled);

    public UpdateProductImagesDtoValidator()
    {
        RuleFor(d => d.Order)
            .Must(order => order.Count <= ProductImageRules.MaxImages)
            .WithMessage($"الحد الأقصى {ProductImageRules.MaxImages} صور للمنتج")
            .Must(order => order.All(token => TokenPattern.IsMatch(token)))
            .WithMessage("ترتيب الصور غير صحيح")
            .Must(order => order.Distinct().Count() == order.Count)
            .WithMessage("ترتيب الصور غير صحيح");

        // كل ملف مرفوع لازم يكون مذكور في الترتيب مرة واحدة بالظبط
        RuleFor(d => d.NewFiles)
            .Must((dto, files) =>
            {
                var newTokens = dto.Order.Where(t => t.StartsWith("n:")).ToList();
                if (newTokens.Count != files.Count) return false;

                return newTokens.All(t => int.TryParse(t[2..], out var index) && index >= 0 && index < files.Count);
            })
            .WithMessage("عدد الصور المرفوعة لا يطابق الترتيب");

        RuleForEach(d => d.NewFiles)
            .Must(file => file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("الملف يجب أن يكون صورة")
            .Must(file => file.Length <= ImageRules.MaxBytes)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت");
    }
}