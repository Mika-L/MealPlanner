using FluentValidation;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

internal sealed class ListMealsValidator : AbstractValidator<ListMealsQuery>
{
    public const int MaxPageSize = 100;

    public ListMealsValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Le numéro de page doit être supérieur ou égal à 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"La taille de page doit être comprise entre 1 et {MaxPageSize}.");
    }
}
