using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.DeleteMeal;

internal sealed class DeleteMealHandler(MealsDbContext dbContext)
    : ICommandHandler<DeleteMealCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMealCommand command, CancellationToken cancellationToken)
    {
        var meal = await dbContext.Meals
            .FirstOrDefaultAsync(meal => meal.Id == command.Id, cancellationToken);

        if (meal is null)
        {
            return Result.Failure(Error.NotFound("Meal.NotFound", "Recette introuvable."));
        }

        dbContext.Meals.Remove(meal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
