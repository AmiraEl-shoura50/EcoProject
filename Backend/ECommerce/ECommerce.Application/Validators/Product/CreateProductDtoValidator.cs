using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Application.Validators.Product;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب")
            .MaximumLength(200).WithMessage("اسم المنتج طويل جدًا (الحد الأقصى 200 حرف)");

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("وصف المنتج مطلوب")
            .MaximumLength(2000).WithMessage("الوصف طويل جدًا");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("السعر لازم يكون أكبر من صفر");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("الكمية المتاحة لا يمكن أن تكون سالبة");

        RuleFor(p => p.CategoryId)
            .GreaterThan(0).WithMessage("القسم مطلوب");
    }
}