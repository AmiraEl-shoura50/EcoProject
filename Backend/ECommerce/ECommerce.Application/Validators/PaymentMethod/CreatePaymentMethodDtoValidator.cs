using ECommerce.Application.DTOs.PaymentMethod;
using FluentValidation;

namespace ECommerce.Application.Validators.PaymentMethod;

public class CreatePaymentMethodDtoValidator : AbstractValidator<CreatePaymentMethodDto>
{
    public CreatePaymentMethodDtoValidator()
    {
        RuleFor(p => p.MethodName)
            .NotEmpty().WithMessage("اسم طريقة الدفع مطلوب")
            .MaximumLength(100);
    }
}