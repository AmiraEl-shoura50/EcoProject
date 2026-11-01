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

        RuleFor(d => d.DiscountAmount)
            .LessThan(100).WithMessage("نسبة الخصم يجب أن تكون أقل من 100%")
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