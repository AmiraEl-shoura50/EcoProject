using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Validators.Order;

public class DeleteOrdersDtoValidator : AbstractValidator<DeleteOrdersDto>
{
    public DeleteOrdersDtoValidator()
    {
        RuleFor(d => d.OrderIds)
            .NotEmpty().WithMessage("اختاري طلب واحد على الأقل")
            .Must(ids => ids.Count <= 50).WithMessage("الحد الأقصى 50 طلب في المرة الواحدة");
    }
}