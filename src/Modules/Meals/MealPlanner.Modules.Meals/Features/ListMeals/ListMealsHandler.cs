using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

internal sealed class ListMealsHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<ListMealsQuery, Result<ListMealsResponse>>
{
    public async Task<Result<ListMealsResponse>> HandleAsync(
        ListMealsQuery query,
        CancellationToken cancellationToken)
    {
        // Chaque utilisateur ne voit que ses propres recettes.
        var owned = dbContext.Meals
            .Where(meal => meal.OwnerId == currentUser.UserId)
            .Include(meal => meal.Ingredients)
            .OrderBy(meal => meal.Name);

        var search = query.Search?.Trim();

        // Sans recherche : pagination côté SQL (total AVANT pagination pour connaître le nombre de pages).
        if (string.IsNullOrEmpty(search))
        {
            var total = await owned.CountAsync(cancellationToken);
            var page = await owned
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return Result.Success(new ListMealsResponse(ToSummaries(page), query.Page, query.PageSize, total));
        }

        // Recherche insensible à la casse ET aux accents : SQLite ne sait pas normaliser les accents en
        // SQL, on matérialise le (petit) catalogue de l'utilisateur puis on filtre/pagine en mémoire.
        var normalizedSearch = SearchText.Normalize(search);
        var matches = (await owned.ToListAsync(cancellationToken))
            .Where(meal =>
                SearchText.Normalize(meal.Name).Contains(normalizedSearch)
                || SearchText.Normalize(meal.Description).Contains(normalizedSearch)
                || meal.Ingredients.Any(ingredient => SearchText.Normalize(ingredient.Name).Contains(normalizedSearch)))
            .ToList();

        var matchesPage = matches
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Result.Success(new ListMealsResponse(ToSummaries(matchesPage), query.Page, query.PageSize, matches.Count));
    }

    private static List<MealSummary> ToSummaries(IEnumerable<Meal> meals) =>
        meals
            .Select(meal => new MealSummary(
                meal.Id,
                meal.Name,
                meal.Description,
                FlagEnum.Decompose(meal.Seasons),
                FlagEnum.Decompose(meal.Styles),
                meal.PrepTimeMinutes,
                meal.Ingredients.Select(ingredient => ingredient.Name).ToList()))
            .ToList();
}
