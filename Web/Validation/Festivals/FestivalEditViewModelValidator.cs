using Domain.ViewModels.Festivals;
using FluentValidation;

public class FestivalEditViewModelValidator : AbstractValidator<FestivalEditViewModel>
{
    public FestivalEditViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Введите название фестиваля")
            .MaximumLength(150).WithMessage("Название слишком длинное");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание слишком длинное");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Название города слишком длинное");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Название страны слишком длинное");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Укажите дату начала фестиваля");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Укажите дату окончания фестиваля")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Дата окончания не может быть раньше даты начала");

        RuleFor(x => x.CoverUrl)
            .NotEmpty().WithMessage("Укажите обложку (URL изображения)")
            .MaximumLength(500).WithMessage("Слишком длинный URL изображения");
    }
}
