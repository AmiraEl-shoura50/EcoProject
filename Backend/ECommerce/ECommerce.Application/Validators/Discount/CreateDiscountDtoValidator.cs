using ECommerce.Application.DTOs.Discount;
using FluentValidation;

namespace ECommerce.Application.Validators.Discount;

public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountDtoValidator()
    {
        RuleFor(d => d.DiscountType)
            .Must(t => t == "Percentage" || t == "FixedAmount")
            .WithMessage("نوع الخصم يجب أن يكون Percentage أو FixedAmount");

        RuleFor(d => d.DiscountAmount)
            .GreaterThan(0).WithMessage("قيمة الخصم لازم تكون أكبر من صفر");

        // ✅ لو النوع Percentage، القيمة لازم تكون بين 1 و 100
        RuleFor(d => d.DiscountAmount)
            .LessThanOrEqualTo(100).WithMessage("نسبة الخصم لا يمكن أن تتجاوز 100%")
            .When(d => d.DiscountType == "Percentage");

        RuleFor(d => d.StartDate)
            .LessThan(d => d.EndDate).WithMessage("تاريخ البداية لازم يكون قبل تاريخ النهاية");

        RuleFor(d => d.TargetType)
            .Must(t => t == "Product" || t == "Category")
            .WithMessage("نوع الهدف يجب أن يكون Product أو Category");

        RuleFor(d => d.TargetId)
            .GreaterThan(0).WithMessage("الهدف مطلوب");
    }
}