using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(f => f.Email)
            .NotEmpty().WithMessage("الإيميل مطلوب")
            .EmailAddress().WithMessage("صيغة الإيميل غير صحيحة");
    }
}