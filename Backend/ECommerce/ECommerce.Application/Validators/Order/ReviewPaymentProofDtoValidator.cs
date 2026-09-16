using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Validators.Order;

public class ReviewPaymentProofDtoValidator : AbstractValidator<ReviewPaymentProofDto>
{
    public ReviewPaymentProofDtoValidator()
    {
        RuleFor(r => r.RejectionReason)
            .NotEmpty().WithMessage("سبب الرفض مطلوب عند الرفض")
            .When(r => !r.Approve);
    }
}