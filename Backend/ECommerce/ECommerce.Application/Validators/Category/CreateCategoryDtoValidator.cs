using ECommerce.Application.DTOs.Category;
using FluentValidation;

namespace ECommerce.Application.Validators.Category;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("اسم القسم مطلوب")
            .MaximumLength(100).WithMessage("اسم القسم طويل جدًا");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("وصف القسم مطلوب")
            .MaximumLength(500);
    }
}