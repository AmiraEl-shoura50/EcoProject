using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Validators.Order;

public class SubmitPaymentProofDtoValidator : AbstractValidator<SubmitPaymentProofDto>
{
    public SubmitPaymentProofDtoValidator()
    {
        RuleFor(p => p.Image).NotNull().WithMessage("صورة إثبات الدفع مطلوبة");
    }
}