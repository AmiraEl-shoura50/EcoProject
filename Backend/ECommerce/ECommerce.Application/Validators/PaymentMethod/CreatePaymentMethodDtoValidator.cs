using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators.PaymentMethod;

public class CreatePaymentMethodDtoValidator : AbstractValidator<CreatePaymentMethodDto>
{
    public CreatePaymentMethodDtoValidator()
    {
        RuleFor(p => p.MethodName)
            .NotEmpty().WithMessage("اسم طريقة الدفع مطلوب")
            .MaximumLength(100).WithMessage("اسم طريقة الدفع طويل جدًا");

        RuleFor(p => p.Type)
            .IsInEnum().WithMessage("نوع طريقة الدفع غير صحيح");

        RuleFor(p => p.ProviderIntegrationId)
            .NotEmpty().WithMessage("رقم التكامل مع بوابة الدفع مطلوب")
            .Matches("^[0-9]+$").WithMessage("رقم التكامل يجب أن يحتوي على أرقام إنجليزية فقط")
            .MaximumLength(100)
            .When(p => p.Type == PaymentMethodType.Gateway);

        RuleFor(p => p.TransferUrl)
            .MaximumLength(500).WithMessage("رابط التحويل طويل جدًا")
            .Must(BeValidWebUrl).WithMessage("رابط التحويل غير صحيح (يجب أن يبدأ بـ http أو https)")
            .When(p => !string.IsNullOrWhiteSpace(p.TransferUrl));
    }

    // بنقبل http و https بس، عشان روابط زي javascript: متتخزنش وتتعرض للعملاء
    private static bool BeValidWebUrl(string? url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}