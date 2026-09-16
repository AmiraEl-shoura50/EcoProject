using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Application.Validators.Product;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب")
            .MaximumLength(200);

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("وصف المنتج مطلوب")
            .MaximumLength(2000);

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("السعر لازم يكون أكبر من صفر");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(p => p.CategoryId)
            .GreaterThan(0);
    }
}