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
            .AsQueryable();

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            // Recherche entièrement côté SQL : les colonnes (nom, description, ingrédient) portent une
            // collation insensible à la casse ET aux accents (voir MealsDbContext.SearchCollation), donc
            // LIKE matche "gruyere" avec "Gruyère" sans matérialiser le catalogue en mémoire.
            owned = owned.Where(meal =>
                meal.Name.Contains(search)
                || meal.Description.Contains(search)
                || meal.Ingredients.Any(ingredient => ingredient.Name.Contains(search)));
        }

        var ordered = owned.OrderBy(meal => meal.Name);

        // Total AVANT pagination pour connaître le nombre de pages.
        var total = await ordered.CountAsync(cancellationToken);
        var page = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new ListMealsResponse(ToSummaries(page), query.Page, query.PageSize, total));
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
