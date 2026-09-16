using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(r => r.FirstName).NotEmpty().WithMessage("الاسم الأول مطلوب");
        RuleFor(r => r.LastName).NotEmpty().WithMessage("الاسم الأخير مطلوب");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("الإيميل مطلوب")
            .EmailAddress().WithMessage("صيغة الإيميل غير صحيحة");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(6).WithMessage("كلمة المرور لازم تكون 6 أحرف على الأقل");

        RuleFor(r => r.Role)
            .Must(role => role == "Customer" || role == "Seller")
            .WithMessage("الدور المسموح به Customer أو Seller بس");

        // ✅ Conditional - لو Role = Customer، الـ Address لازم تكون موجودة
        RuleFor(r => r.Address)
            .NotEmpty().WithMessage("العنوان مطلوب لحساب العميل")
            .When(r => r.Role == "Customer");

        // ✅ Conditional - لو Role = Seller، الـ StoreName لازم تكون موجودة
        RuleFor(r => r.StoreName)
            .NotEmpty().WithMessage("اسم المتجر مطلوب لحساب البائع")
            .When(r => r.Role == "Seller");
    }
}