using Domain.ViewModels.LoginAndRegistration;
using FluentValidation;

namespace DaviskibaYP.Validation.LoginAndRegistration
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Введите имя")
                .MaximumLength(50).WithMessage("Имя не должно быть длиннее 50 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите email")
                .EmailAddress().WithMessage("Некорректный email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введите пароль")
                .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов");

            // этот блок оставляем ТОЛЬКО если у тебя есть ConfirmPassword
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Повторите пароль")
                .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        }
    }
}
