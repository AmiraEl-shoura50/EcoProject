using ECommerce.Application.DTOs.Wishlist;
using FluentValidation;

namespace ECommerce.Application.Validators.Wishlist;

public class AddToWishlistDtoValidator : AbstractValidator<AddToWishlistDto>
{
    public AddToWishlistDtoValidator()
    {
        RuleFor(w => w.ProductId)
            .GreaterThan(0).WithMessage("المنتج مطلوب");
    }
}