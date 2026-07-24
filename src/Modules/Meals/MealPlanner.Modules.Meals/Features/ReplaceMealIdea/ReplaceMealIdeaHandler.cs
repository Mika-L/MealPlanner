using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

internal sealed class ReplaceMealIdeaHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<ReplaceMealIdeaQuery, Result<ReplaceMealIdeaResponse>>
{
    public async Task<Result<ReplaceMealIdeaResponse>> HandleAsync(
        ReplaceMealIdeaQuery query,
        CancellationToken cancellationToken)
    {
        // La recette écartée et les repas conservés ne doivent pas être repiochés : on veut « une autre ».
        var excludedIds = query.KeptMealIds.Append(query.ReplacedMealId).ToArray();

        // Mêmes filtres déterministes que la génération d'idées, sur le catalogue de l'utilisateur.
        var candidatesQuery = dbContext.Meals
            .Where(meal => meal.OwnerId == currentUser.UserId)
            .Where(meal => !excludedIds.Contains(meal.Id))
            .Include(meal => meal.Ingredients)
            .AsQueryable();

        if (query.Season is { } season && season != Season.None)
        {
            candidatesQuery = candidatesQuery.Where(meal => (meal.Seasons & season) != Season.None);
        }

        if (query.Styles is { } styles && styles != MealStyle.None)
        {
            candidatesQuery = candidatesQuery.Where(meal => (meal.Styles & styles) != MealStyle.None);
        }

        if (query.MaxPrepTimeMinutes is { } maxPrepTime)
        {
            candidatesQuery = candidatesQuery.Where(meal => meal.PrepTimeMinutes <= maxPrepTime);
        }

        // Les plus rapides d'abord : même ordre de priorité que la génération d'idées.
        var candidates = await candidatesQuery
            .OrderBy(meal => meal.PrepTimeMinutes)
            .ToListAsync(cancellationToken);

        var replacement = query.IncludeIngredients is { Count: > 0 } includeIngredients
            ? await SelectWithPantryAsync(candidates, includeIngredients, query.KeptMealIds, cancellationToken)
            : candidates.Count > 0 ? new Selection(candidates[0], []) : null;

        if (replacement is null)
        {
            return Result.Failure<ReplaceMealIdeaResponse>(
                Error.NotFound("MealIdea.NoReplacement", "Aucune autre recette ne correspond à ces critères."));
        }

        var meal = replacement.Meal;
        var plannedMeal = new PlannedMeal(
            query.Day,
            meal.Id,
            meal.Name,
            meal.Description,
            meal.PrepTimeMinutes,
            FlagEnum.Decompose(meal.Styles),
            meal.Ingredients.Select(ingredient => ingredient.Name).ToList(),
            replacement.Matched);

        return Result.Success(new ReplaceMealIdeaResponse(plannedMeal));
    }

    // Reproduit l'allocation « un ingrédient = un repas » de la génération d'idées, pour un seul jour :
    // priorité à une recette cuisinable avec un ingrédient encore disponible (aucun de ses ingrédients frigo
    // déjà consommé par un repas conservé), sinon une recette neutre (aucun ingrédient du frigo).
    private async Task<Selection?> SelectWithPantryAsync(
        IReadOnlyList<Meal> candidates,
        IReadOnlyList<string> availableIngredients,
        IReadOnlyList<Guid> keptMealIds,
        CancellationToken cancellationToken)
    {
        var pantry = PantryMatcher.BuildPantry(availableIngredients);

        // Ingrédients du frigo déjà consommés par les repas conservés du planning.
        var keptIds = keptMealIds.ToArray();
        var keptMeals = await dbContext.Meals
            .Where(meal => meal.OwnerId == currentUser.UserId && keptIds.Contains(meal.Id))
            .Include(meal => meal.Ingredients)
            .ToListAsync(cancellationToken);

        var consumed = keptMeals
            .SelectMany(meal => PantryMatcher.Match(meal, pantry))
            .Select(term => term.Normalized)
            .ToHashSet();

        var scored = candidates
            .Select(meal => new { Meal = meal, Matched = PantryMatcher.Match(meal, pantry) })
            .ToList();

        // 1. Une recette dont tous les ingrédients frigo sont encore disponibles.
        var withIngredient = scored.FirstOrDefault(candidate =>
            candidate.Matched.Count > 0 && !candidate.Matched.Any(term => consumed.Contains(term.Normalized)));
        if (withIngredient is not null)
        {
            return new Selection(withIngredient.Meal, withIngredient.Matched.Select(term => term.Original).ToList());
        }

        // 2. Sinon une recette neutre (n'utilise aucun ingrédient du frigo).
        var neutral = scored.FirstOrDefault(candidate => candidate.Matched.Count == 0);
        return neutral is null ? null : new Selection(neutral.Meal, []);
    }

    private sealed record Selection(Meal Meal, IReadOnlyList<string> Matched);
}
