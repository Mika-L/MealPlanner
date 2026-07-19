using FluentValidation;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

internal sealed class GenerateMealIdeasValidator : AbstractValidator<GenerateMealIdeasQuery>
{
    public GenerateMealIdeasValidator()
    {
        RuleFor(query => query.Days)
            .InclusiveBetween(1, 30)
            .WithMessage("Le nombre de jours doit être compris entre 1 et 30.");

        RuleFor(query => query.MaxPrepTimeMinutes)
            .GreaterThan(0)
            .When(query => query.MaxPrepTimeMinutes.HasValue)
            .WithMessage("Le temps de préparation maximum doit être positif.");
    }
}
