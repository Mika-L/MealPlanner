using FluentValidation;

namespace MealPlanner.Modules.Meals.Features.CreateMeal;

internal sealed class CreateMealValidator : AbstractValidator<CreateMealCommand>
{
    public CreateMealValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom ne peut pas dépasser 200 caractères.");

        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères.");

        RuleFor(command => command.PrepTimeMinutes)
            .GreaterThan(0).WithMessage("Le temps de préparation doit être positif.");

        RuleForEach(command => command.Ingredients)
            .NotEmpty().WithMessage("Un ingrédient ne peut pas être vide.")
            .MaximumLength(200).WithMessage("Un ingrédient ne peut pas dépasser 200 caractères.");
    }
}
