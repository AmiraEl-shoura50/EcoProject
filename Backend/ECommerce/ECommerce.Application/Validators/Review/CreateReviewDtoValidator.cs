using ECommerce.Application.DTOs.Review;
using FluentValidation;

namespace ECommerce.Application.Validators.Review;

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(r => r.ProductId).GreaterThan(0).WithMessage("المنتج مطلوب");
        RuleFor(r => r.OrderId).GreaterThan(0).WithMessage("الطلب مطلوب");

        RuleFor(r => r.Rating)
            .InclusiveBetween(1, 5).WithMessage("التقييم لازم يكون بين 1 و 5");

        RuleFor(r => r.Comment)
            .MaximumLength(1000).WithMessage("التعليق طويل جدًا");
    }
}