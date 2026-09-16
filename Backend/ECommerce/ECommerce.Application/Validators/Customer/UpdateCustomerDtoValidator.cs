using ECommerce.Application.DTOs.Customer;
using FluentValidation;

namespace ECommerce.Application.Validators.Customer;

public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerDtoValidator()
    {
        RuleFor(c => c.Address)
            .NotEmpty().WithMessage("العنوان مطلوب")
            .MaximumLength(300);
    }
}