using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

/// <summary>
/// Liste le catalogue de recettes page par page, filtré par un terme de recherche optionnel
/// (nom, description ou ingrédient). Pensé pour des catalogues de plusieurs centaines de recettes.
/// </summary>
public sealed record ListMealsQuery(string? Search, int Page, int PageSize) : IQuery<Result<ListMealsResponse>>;

/// <summary>Page de résultats accompagnée du total filtré, pour piloter la pagination côté client.</summary>
public sealed record ListMealsResponse(
    IReadOnlyList<MealSummary> Meals,
    int Page,
    int PageSize,
    int Total);

/// <summary>Vue d'une recette pour la gestion (saisons et styles éclatés en valeurs atomiques).</summary>
public sealed record MealSummary(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<MealStyle> Styles,
    int PrepTimeMinutes,
    IReadOnlyList<string> Ingredients);
