using ECommerce.Application.DTOs.Cart;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart;

public class UpdateCartItemDtoValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemDtoValidator()
    {
        RuleFor(c => c.Quantity)
            .GreaterThan(0).WithMessage("الكمية لازم تكون أكبر من صفر");
    }
}