using FluentValidation;

namespace MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

internal sealed class ReplaceMealIdeaValidator : AbstractValidator<ReplaceMealIdeaQuery>
{
    public ReplaceMealIdeaValidator()
    {
        RuleFor(query => query.Day)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Le jour à remplacer doit être positif.");

        RuleFor(query => query.MaxPrepTimeMinutes)
            .GreaterThan(0)
            .When(query => query.MaxPrepTimeMinutes.HasValue)
            .WithMessage("Le temps de préparation maximum doit être positif.");
    }
}
