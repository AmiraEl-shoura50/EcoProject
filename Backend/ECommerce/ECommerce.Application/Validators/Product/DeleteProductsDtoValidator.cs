using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Application.Validators.Product;

public class DeleteProductsDtoValidator : AbstractValidator<DeleteProductsDto>
{
    public DeleteProductsDtoValidator()
    {
        RuleFor(d => d.ProductIds)
            .NotEmpty().WithMessage("اختر منتجًا واحدًا على الأقل")
            .Must(ids => ids.Count <= 50).WithMessage("الحد الأقصى 50 منتجًا في المرة الواحدة");
    }
}