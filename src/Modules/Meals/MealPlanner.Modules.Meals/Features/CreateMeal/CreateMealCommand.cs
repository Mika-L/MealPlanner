using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.CreateMeal;

/// <summary>Ajoute une nouvelle recette au catalogue et renvoie son identifiant.</summary>
public sealed record CreateMealCommand(
    string Name,
    string Description,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<MealStyle> Styles,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients) : ICommand<Result<Guid>>;
