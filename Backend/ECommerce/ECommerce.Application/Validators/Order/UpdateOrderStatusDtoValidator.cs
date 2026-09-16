using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Validators.Order;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(o => o.NewStatus)
            .Must(s => new[] { "Confirmed", "Shipped", "Delivered", "Cancelled" }.Contains(s))
            .WithMessage("حالة الطلب غير صحيحة");
    }
}