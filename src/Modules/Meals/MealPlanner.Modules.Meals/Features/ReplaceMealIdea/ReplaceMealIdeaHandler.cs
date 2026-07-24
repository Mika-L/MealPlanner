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
        var pantry = query.IncludeIngredients is { Count: > 0 } includeIngredients
            ? PantryMatcher.BuildPantry(includeIngredients)
            : null;

        // Ingrédients du frigo déjà consommés par les repas conservés (règle « un ingrédient = un repas »).
        var consumed = pantry is null
            ? new HashSet<string>()
            : await BuildConsumedIngredientsAsync(query.KeptMealIds, pantry, cancellationToken);

        // La recette affichée et les repas conservés ne sont jamais repiochés.
        var pinnedIds = query.KeptMealIds.Append(query.ReplacedMealId).ToArray();

        // 1. On propose en priorité une recette encore jamais montrée sur le planning : « une autre à chaque fois ».
        // 2. Repli : une fois toutes les alternatives épuisées, on relance le cycle depuis le début — sans
        //    jamais reproposer la recette actuellement affichée (elle reste écartée via pinnedIds).
        var freshlyExcluded = pinnedIds.Concat(query.SeenMealIds).Distinct().ToArray();

        var replacement =
            await SelectReplacementAsync(query, freshlyExcluded, pantry, consumed, cancellationToken)
            ?? await SelectReplacementAsync(query, pinnedIds, pantry, consumed, cancellationToken);

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

    // Sélectionne une recette éligible en écartant les identifiants fournis. Applique les mêmes filtres
    // déterministes que la génération d'idées, puis (si le frigo est renseigné) l'allocation
    // « un ingrédient = un repas » pour un seul jour : priorité à une recette cuisinable avec un ingrédient
    // encore disponible (aucun de ses ingrédients frigo déjà consommé par un repas conservé), sinon une
    // recette neutre (aucun ingrédient du frigo).
    private async Task<Selection?> SelectReplacementAsync(
        ReplaceMealIdeaQuery query,
        IReadOnlyList<Guid> excludedIds,
        IReadOnlyList<PantryTerm>? pantry,
        IReadOnlySet<string> consumed,
        CancellationToken cancellationToken)
    {
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

        if (pantry is null)
        {
            return candidates.Count > 0 ? new Selection(candidates[0], []) : null;
        }

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

    // Ingrédients du frigo déjà consommés par les repas conservés du planning.
    private async Task<HashSet<string>> BuildConsumedIngredientsAsync(
        IReadOnlyList<Guid> keptMealIds,
        IReadOnlyList<PantryTerm> pantry,
        CancellationToken cancellationToken)
    {
        var keptIds = keptMealIds.ToArray();
        var keptMeals = await dbContext.Meals
            .Where(meal => meal.OwnerId == currentUser.UserId && keptIds.Contains(meal.Id))
            .Include(meal => meal.Ingredients)
            .ToListAsync(cancellationToken);

        return keptMeals
            .SelectMany(meal => PantryMatcher.Match(meal, pantry))
            .Select(term => term.Normalized)
            .ToHashSet();
    }

    private sealed record Selection(Meal Meal, IReadOnlyList<string> Matched);
}
