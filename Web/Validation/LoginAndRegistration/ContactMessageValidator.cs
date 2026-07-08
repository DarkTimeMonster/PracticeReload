using Domain.Entities;
using FluentValidation;

namespace DaviskibaYP.Validators
{
    public class ContactMessageValidator : AbstractValidator<ContactMessage>
    {
        public ContactMessageValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя обязательно")
                .MaximumLength(100).WithMessage("Имя не должно быть длиннее 100 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Введите корректный Email")
                .MaximumLength(150).WithMessage("Email не должен быть длиннее 150 символов");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Сообщение обязательно")
                .MinimumLength(10).WithMessage("Сообщение должно содержать минимум 10 символов")
                .MaximumLength(3000).WithMessage("Сообщение не должно быть длиннее 3000 символов");
        }
    }
}
