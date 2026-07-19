using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

/// <summary>
/// Génère un planning de repas pour les prochains jours selon des critères (saison, style, temps),
/// en priorisant les repas cuisinables avec les ingrédients disponibles.
/// </summary>
public sealed record GenerateMealIdeasQuery(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int Days) : IQuery<Result<GenerateMealIdeasResponse>>;

public sealed record GenerateMealIdeasResponse(IReadOnlyList<PlannedMeal> Ideas);

/// <summary>Un repas positionné sur un jour du planning (jour 1 = aujourd'hui).</summary>
public sealed record PlannedMeal(
    int Day,
    Guid Id,
    string Name,
    string Description,
    int PrepTimeMinutes,
    IReadOnlyList<MealStyle> Styles,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> MatchedIngredients);
