using ECommerce.Application.DTOs.Cart;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart;

public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartDtoValidator()
    {
        RuleFor(c => c.ProductId)
            .GreaterThan(0).WithMessage("المنتج مطلوب");

        RuleFor(c => c.Quantity)
            .GreaterThan(0).WithMessage("الكمية لازم تكون أكبر من صفر");
    }
}