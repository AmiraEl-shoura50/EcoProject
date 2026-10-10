using ECommerce.Application.DTOs.Customer;
using FluentValidation;

namespace ECommerce.Application.Validators.Customer;

public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerDtoValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MaximumLength(50).WithMessage("الاسم الأول طويل جدًا");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("الاسم الأخير مطلوب")
            .MaximumLength(50).WithMessage("الاسم الأخير طويل جدًا");

        RuleFor(c => c.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .Matches(@"^\+?[0-9]{8,15}$").WithMessage("رقم الهاتف غير صحيح");

        RuleFor(c => c.Address)
            .NotEmpty().WithMessage("العنوان مطلوب")
            .MaximumLength(300).WithMessage("العنوان طويل جدًا");
    }
}