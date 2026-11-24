using System.Text.RegularExpressions;
using ECommerce.Application.DTOs.Seller;
using ECommerce.Application.Validators.Common;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators.Seller;

public class UpdateSellerDtoValidator : AbstractValidator<UpdateSellerDto>
{
    private static readonly Regex WalletPattern = new(@"^01[0125][0-9]{8}$", RegexOptions.Compiled);

    public UpdateSellerDtoValidator()
    {
        RuleFor(s => s.StoreName)
            .NotEmpty().WithMessage("اسم المتجر مطلوب")
            .MaximumLength(150).WithMessage("اسم المتجر طويل جدًا");

        RuleFor(s => s.Image).MustBeValidImage();

        RuleFor(s => s.PayoutMethod)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PayoutMethod>(value, out _))
            .WithMessage("طريقة التحويل غير صحيحة");

        RuleFor(s => s.PayoutAccount)
            .NotEmpty().WithMessage("أدخل رقم أو عنوان التحويل")
            .When(s => !string.IsNullOrWhiteSpace(s.PayoutMethod));

        RuleFor(s => s.PayoutAccount)
            .NotEmpty().WithMessage("اختر طريقة التحويل")
            .When(s => string.IsNullOrWhiteSpace(s.PayoutMethod) && !string.IsNullOrWhiteSpace(s.PayoutAccount));

        RuleFor(s => s.PayoutAccount)
            .MaximumLength(100).WithMessage("بيانات التحويل طويلة جدًا");

        RuleFor(s => s.PayoutAccount)
            .Must(value => value is not null && WalletPattern.IsMatch(value.Trim()))
            .WithMessage("رقم فودافون كاش غير صحيح (11 رقمًا يبدأ بـ 01)")
            .When(s => s.PayoutMethod == nameof(PayoutMethod.VodafoneCash));
    }
}