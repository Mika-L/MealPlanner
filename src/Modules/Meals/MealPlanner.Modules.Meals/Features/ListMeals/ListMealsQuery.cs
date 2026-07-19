using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

/// <summary>Liste l'intégralité du catalogue de recettes, pour la section de gestion.</summary>
public sealed record ListMealsQuery : IQuery<Result<ListMealsResponse>>;

public sealed record ListMealsResponse(IReadOnlyList<MealSummary> Meals);

/// <summary>Vue d'une recette pour la gestion (saisons et styles éclatés en valeurs atomiques).</summary>
public sealed record MealSummary(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<MealStyle> Styles,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients);
