using ECommerce.Application.Constants;
using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Application.Validators.Product;

public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockDtoValidator()
    {
        RuleFor(d => d.Mode)
            .Must(mode => mode is "add" or "set")
            .WithMessage("نوع التعديل يجب أن يكون add أو set");

        RuleFor(d => d.Quantity)
            .InclusiveBetween(1, StockRules.MaxStock)
            .WithMessage("كمية الإضافة يجب أن تكون بين 1 و 1,000,000")
            .When(d => d.Mode == "add");

        RuleFor(d => d.Quantity)
            .InclusiveBetween(0, StockRules.MaxStock)
            .WithMessage("الكمية يجب أن تكون بين 0 و 1,000,000")
            .When(d => d.Mode == "set");
    }
}