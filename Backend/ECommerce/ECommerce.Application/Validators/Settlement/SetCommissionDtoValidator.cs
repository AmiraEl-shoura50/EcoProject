using ECommerce.Application.DTOs.Settlement;
using FluentValidation;

namespace ECommerce.Application.Validators.Settlement;

public class SetCommissionDtoValidator : AbstractValidator<SetCommissionDto>
{
    public SetCommissionDtoValidator()
    {
        RuleFor(d => d.CommissionPercent)
            .InclusiveBetween(0m, 100m).WithMessage("نسبة العمولة يجب أن تكون بين 0 و 100")
            .PrecisionScale(5, 2, false).WithMessage("نسبة العمولة حتى خانتين عشريتين");
    }
}