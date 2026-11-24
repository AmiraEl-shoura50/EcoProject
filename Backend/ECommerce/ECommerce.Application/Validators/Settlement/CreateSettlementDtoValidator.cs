using System.Globalization;
using ECommerce.Application.DTOs.Settlement;
using FluentValidation;

namespace ECommerce.Application.Validators.Settlement;

public class CreateSettlementDtoValidator : AbstractValidator<CreateSettlementDto>
{
    public CreateSettlementDtoValidator()
    {
        RuleFor(d => d.Cutoff)
            .Must(value => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            .WithMessage("تاريخ الإغلاق غير صحيح");

        RuleFor(d => d.TzOffset).InclusiveBetween(-840, 840);

        RuleFor(d => d.ExpectedOrdersCount).GreaterThan(0).WithMessage("لا توجد طلبات للتسوية");

        RuleFor(d => d.AdjustmentAmount)
            .InclusiveBetween(-1_000_000m, 1_000_000m).WithMessage("قيمة التعديل غير منطقية");

        RuleFor(d => d.AdjustmentNote)
            .NotEmpty().WithMessage("اكتب سبب التعديل")
            .When(d => d.AdjustmentAmount != 0);

        RuleFor(d => d.AdjustmentNote).MaximumLength(300).WithMessage("سبب التعديل طويل جدًا");
        RuleFor(d => d.Reference).MaximumLength(100).WithMessage("رقم العملية طويل جدًا");
    }
}