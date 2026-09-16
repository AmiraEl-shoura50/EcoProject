using ECommerce.Application.DTOs.Seller;
using FluentValidation;

namespace ECommerce.Application.Validators.Seller;

public class UpdateSellerDtoValidator : AbstractValidator<UpdateSellerDto>
{
    public UpdateSellerDtoValidator()
    {
        RuleFor(s => s.StoreName)
            .NotEmpty().WithMessage("اسم المتجر مطلوب")
            .MaximumLength(150).WithMessage("اسم المتجر طويل جدًا");
    }
}