using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

internal sealed class ListMealsHandler(MealsDbContext dbContext)
    : IQueryHandler<ListMealsQuery, Result<ListMealsResponse>>
{
    public async Task<Result<ListMealsResponse>> HandleAsync(
        ListMealsQuery query,
        CancellationToken cancellationToken)
    {
        var filtered = dbContext.Meals.AsQueryable();

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            // Contains -> LIKE '%term%' ; la collation par défaut (utf8mb4_0900_ai_ci) rend la
            // comparaison insensible à la casse et aux accents. Le filtre sur les ingrédients devient
            // un EXISTS, ce qui évite de matérialiser le catalogue.
            filtered = filtered.Where(meal =>
                meal.Name.Contains(search)
                || meal.Description.Contains(search)
                || meal.Ingredients.Any(ingredient => ingredient.Name.Contains(search)));
        }

        // Total AVANT pagination : le client en a besoin pour savoir s'il reste des pages.
        var total = await filtered.CountAsync(cancellationToken);

        var meals = await filtered
            .Include(meal => meal.Ingredients)
            .OrderBy(meal => meal.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var summaries = meals
            .Select(meal => new MealSummary(
                meal.Id,
                meal.Name,
                meal.Description,
                FlagEnum.Decompose(meal.Seasons),
                FlagEnum.Decompose(meal.Styles),
                meal.PrepTimeMinutes,
                meal.Ingredients.Select(ingredient => ingredient.Name).ToList()))
            .ToList();

        return Result.Success(new ListMealsResponse(summaries, query.Page, query.PageSize, total));
    }
}
