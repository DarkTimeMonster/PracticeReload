using Domain.ViewModels.Festivals;
using FluentValidation;

public class FestivalImageEditViewModelValidator : AbstractValidator<FestivalImageEditViewModel>
{
    public FestivalImageEditViewModelValidator()
    {
        RuleFor(x => x.FestivalId)
            .GreaterThan(0).WithMessage("Неверный идентификатор фестиваля");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Укажите ссылку на картинку")
            .MaximumLength(500).WithMessage("Слишком длинный URL");

        RuleFor(x => x.Alt)
            .MaximumLength(200).WithMessage("Alt-текст слишком длинный");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Порядок должен быть 0 или больше");
    }
}
