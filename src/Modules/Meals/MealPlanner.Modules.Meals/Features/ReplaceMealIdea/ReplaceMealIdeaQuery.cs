using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

/// <summary>
/// Remplace une idée d'un planning par une autre recette respectant les mêmes critères (saison, style,
/// temps) et sans réutiliser les ingrédients du frigo déjà consommés par les repas conservés
/// (règle « un ingrédient = un repas »). La recette écartée et les repas conservés ne sont jamais repiochés.
/// </summary>
public sealed record ReplaceMealIdeaQuery(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int Day,
    Guid ReplacedMealId,
    IReadOnlyList<Guid> KeptMealIds) : IQuery<Result<ReplaceMealIdeaResponse>>;

public sealed record ReplaceMealIdeaResponse(PlannedMeal Meal);
