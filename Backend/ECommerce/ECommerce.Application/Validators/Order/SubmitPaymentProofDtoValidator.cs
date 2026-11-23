using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Validators.Common;
using FluentValidation;

namespace ECommerce.Application.Validators.Order;

public class SubmitPaymentProofDtoValidator : AbstractValidator<SubmitPaymentProofDto>
{
    public SubmitPaymentProofDtoValidator()
    {
        RuleFor(p => p.Image).NotNull().WithMessage("صورة إثبات الدفع مطلوبة");

        RuleFor(p => p.Image)
            .Must(file => file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("الملف يجب أن يكون صورة")
            .Must(file => file.Length <= ImageRules.MaxBytes)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت")
            .When(p => p.Image is not null);

        RuleFor(p => p.TransferReference)
            .MaximumLength(100).WithMessage("رقم العملية طويل جدًا");
    }
}