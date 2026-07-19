using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

/// <summary>Génère des idées de repas selon des critères (saison, style, temps, ingrédients).</summary>
public sealed record GenerateMealIdeasQuery(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int Count) : IQuery<Result<GenerateMealIdeasResponse>>;

public sealed record GenerateMealIdeasResponse(IReadOnlyList<MealIdea> Ideas);

public sealed record MealIdea(
    Guid Id,
    string Name,
    string Description,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients);
