using MealPlanner.Modules.Meals.Domain;

namespace MealPlanner.Modules.Meals.Features;

/// <summary>Corps HTTP commun à la création et à la modification d'une recette.</summary>
public sealed record MealWriteRequest(
    string Name,
    string Description,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<MealStyle> Styles,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients);
