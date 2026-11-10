using ECommerce.Application.DTOs.Contact;
using FluentValidation;

namespace ECommerce.Application.Validators.Contact;

public class ContactMessageDtoValidator : AbstractValidator<ContactMessageDto>
{
    public ContactMessageDtoValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("الاسم مطلوب")
            .MaximumLength(100).WithMessage("الاسم طويل جدًا");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(150).WithMessage("البريد الإلكتروني طويل جدًا");

        RuleFor(c => c.Subject)
            .NotEmpty().WithMessage("الموضوع مطلوب")
            .MaximumLength(150).WithMessage("الموضوع طويل جدًا");

        RuleFor(c => c.Message)
            .NotEmpty().WithMessage("الرسالة مطلوبة")
            .MinimumLength(10).WithMessage("الرسالة قصيرة جدًا (10 أحرف على الأقل)")
            .MaximumLength(2000).WithMessage("الرسالة طويلة جدًا (2000 حرف كحد أقصى)");
    }
}