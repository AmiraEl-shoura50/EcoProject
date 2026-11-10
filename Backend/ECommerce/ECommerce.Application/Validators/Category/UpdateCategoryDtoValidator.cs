using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Validators.Common;
using FluentValidation;

namespace ECommerce.Application.Validators.Category;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("اسم القسم مطلوب")
            .MaximumLength(100).WithMessage("اسم القسم طويل جدًا");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("وصف القسم مطلوب")
            .MaximumLength(500);
        RuleFor(p => p.Image).MustBeValidImage();   // في validators المنتجات
        RuleFor(c => c.Image).MustBeValidImage();   // في validators الأقسام
    }
}