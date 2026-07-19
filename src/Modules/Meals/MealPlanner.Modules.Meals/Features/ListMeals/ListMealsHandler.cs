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
        var meals = await dbContext.Meals
            .Include(meal => meal.Ingredients)
            .OrderBy(meal => meal.Name)
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

        return Result.Success(new ListMealsResponse(summaries));
    }
}
