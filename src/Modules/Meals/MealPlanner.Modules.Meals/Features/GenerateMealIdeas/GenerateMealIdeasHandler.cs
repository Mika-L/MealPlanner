using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

internal sealed class GenerateMealIdeasHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GenerateMealIdeasQuery, Result<GenerateMealIdeasResponse>>
{
    public async Task<Result<GenerateMealIdeasResponse>> HandleAsync(
        GenerateMealIdeasQuery query,
        CancellationToken cancellationToken)
    {
        // Les idées ne piochent que dans le catalogue de l'utilisateur courant.
        var meals = dbContext.Meals
            .Where(meal => meal.OwnerId == currentUser.UserId)
            .Include(meal => meal.Ingredients)
            .AsQueryable();

        if (query.Season is { } season && season != Season.None)
        {
            meals = meals.Where(meal => (meal.Seasons & season) != Season.None);
        }

        if (query.Styles is { } styles && styles != MealStyle.None)
        {
            meals = meals.Where(meal => (meal.Styles & styles) != MealStyle.None);
        }

        if (query.MaxPrepTimeMinutes is { } maxPrepTime)
        {
            meals = meals.Where(meal => meal.PrepTimeMinutes <= maxPrepTime);
        }

        // Les plus rapides d'abord : c'est aussi l'ordre de priorité de l'allocation par ingrédient.
        var ordered = meals.OrderBy(meal => meal.PrepTimeMinutes);

        List<PlanEntry> plan;
        if (query.IncludeIngredients is { Count: > 0 } includeIngredients)
        {
            // La correspondance souple et l'allocation « un ingrédient = un repas » ne s'expriment pas
            // en SQL : on matérialise les candidats (catalogue restreint) puis on construit le planning.
            var pool = await ordered.ToListAsync(cancellationToken);
            plan = BuildPlan(pool, includeIngredients, query.Days);
        }
        else
        {
            var pool = await ordered.Take(query.Days).ToListAsync(cancellationToken);
            plan = pool.Select(meal => new PlanEntry(meal, [])).ToList();
        }

        var ideas = plan
            .Select((entry, index) => new PlannedMeal(
                index + 1,
                entry.Meal.Id,
                entry.Meal.Name,
                entry.Meal.Description,
                entry.Meal.PrepTimeMinutes,
                FlagEnum.Decompose(entry.Meal.Styles),
                entry.Meal.Ingredients.Select(ingredient => ingredient.Name).ToList(),
                entry.Matched))
            .ToList();

        return Result.Success(new GenerateMealIdeasResponse(ideas));
    }

    // Construit le planning en deux temps. D'abord les repas cuisinables avec les ingrédients
    // disponibles (un même ingrédient ne sert qu'un seul repas — une tranche de jambon ne fait pas
    // deux plats). Puis on complète les jours restants, mais uniquement avec des repas n'utilisant
    // AUCUN ingrédient du frigo : on ne réintroduit jamais un repas écarté faute d'ingrédient dispo
    // (pas de « salade au jambon » une fois le jambon consommé par une autre recette). Le pool arrive
    // déjà trié par temps de préparation croissant, ce qui fixe l'ordre de priorité.
    private static List<PlanEntry> BuildPlan(
        IReadOnlyList<Meal> pool,
        IReadOnlyList<string> availableIngredients,
        int days)
    {
        var pantry = PantryMatcher.BuildPantry(availableIngredients);

        var candidates = pool
            .Select(meal => new
            {
                Meal = meal,
                Matched = PantryMatcher.Match(meal, pantry),
            })
            .ToList();

        var consumed = new HashSet<string>();
        var plan = new List<PlanEntry>();

        // 1. Priorité : repas s'appuyant sur des ingrédients disponibles encore non consommés.
        foreach (var candidate in candidates)
        {
            if (plan.Count == days)
            {
                break;
            }

            if (candidate.Matched.Count == 0 || candidate.Matched.Any(term => consumed.Contains(term.Normalized)))
            {
                continue;
            }

            plan.Add(new PlanEntry(candidate.Meal, candidate.Matched.Select(term => term.Original).ToList()));
            foreach (var term in candidate.Matched)
            {
                consumed.Add(term.Normalized);
            }
        }

        // 2. Complément : on remplit les jours restants avec les repas neutres (aucun ingrédient du frigo).
        foreach (var candidate in candidates)
        {
            if (plan.Count == days)
            {
                break;
            }

            if (candidate.Matched.Count == 0)
            {
                plan.Add(new PlanEntry(candidate.Meal, []));
            }
        }

        return plan;
    }

    private sealed record PlanEntry(Meal Meal, IReadOnlyList<string> Matched);
}
