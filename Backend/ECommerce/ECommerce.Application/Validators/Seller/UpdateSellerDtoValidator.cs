using ECommerce.Application.DTOs.Seller;
using FluentValidation;

namespace ECommerce.Application.Validators.Seller;

public class UpdateSellerDtoValidator : AbstractValidator<UpdateSellerDto>
{
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5MB

    public UpdateSellerDtoValidator()
    {
        RuleFor(s => s.StoreName)
            .NotEmpty().WithMessage("اسم المتجر مطلوب")
            .MaximumLength(150).WithMessage("اسم المتجر طويل جدًا");

        RuleFor(s => s.Image)
            .Must(file => file!.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("الملف يجب أن يكون صورة")
            .Must(file => file!.Length <= MaxImageBytes)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت")
            .When(s => s.Image is not null);
    }
}