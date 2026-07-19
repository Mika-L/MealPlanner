using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.UpdateMeal;

/// <summary>Met à jour une recette existante (identifiée par <see cref="Id"/>).</summary>
public sealed record UpdateMealCommand(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<MealStyle> Styles,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients) : ICommand<Result>;
