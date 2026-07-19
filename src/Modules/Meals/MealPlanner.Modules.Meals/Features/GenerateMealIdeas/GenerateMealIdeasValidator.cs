using FluentValidation;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

internal sealed class GenerateMealIdeasValidator : AbstractValidator<GenerateMealIdeasQuery>
{
    public GenerateMealIdeasValidator()
    {
        RuleFor(query => query.Count)
            .InclusiveBetween(1, 50)
            .WithMessage("Le nombre d'idées doit être compris entre 1 et 50.");

        RuleFor(query => query.MaxPrepTimeMinutes)
            .GreaterThan(0)
            .When(query => query.MaxPrepTimeMinutes.HasValue)
            .WithMessage("Le temps de préparation maximum doit être positif.");
    }
}
