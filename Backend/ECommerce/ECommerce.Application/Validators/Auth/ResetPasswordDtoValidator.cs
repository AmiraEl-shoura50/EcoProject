using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("الإيميل مطلوب")
            .EmailAddress().WithMessage("صيغة الإيميل غير صحيحة");

        RuleFor(r => r.Token)
            .NotEmpty().WithMessage("الرابط غير صالح");

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(6).WithMessage("كلمة المرور لازم تكون 6 أحرف على الأقل");
    }
}