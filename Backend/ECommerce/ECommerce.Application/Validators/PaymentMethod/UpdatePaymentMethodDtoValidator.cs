using ECommerce.Application.DTOs.PaymentMethod;
using FluentValidation;

namespace ECommerce.Application.Validators.PaymentMethod;

public class UpdatePaymentMethodDtoValidator : AbstractValidator<UpdatePaymentMethodDto>
{
    public UpdatePaymentMethodDtoValidator()
    {
        RuleFor(p => p.MethodName)
            .NotEmpty().WithMessage("اسم طريقة الدفع مطلوب")
            .MaximumLength(100).WithMessage("اسم طريقة الدفع طويل جدًا");

        RuleFor(p => p.TransferUrl)
            .MaximumLength(500).WithMessage("رابط التحويل طويل جدًا")
            .Must(BeValidWebUrl).WithMessage("رابط التحويل غير صحيح (يجب أن يبدأ بـ http أو https)")
            .When(p => !string.IsNullOrWhiteSpace(p.TransferUrl));
    }

    private static bool BeValidWebUrl(string? url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}