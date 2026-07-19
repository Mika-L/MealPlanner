using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.DeleteMeal;

internal sealed class DeleteMealHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<DeleteMealCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMealCommand command, CancellationToken cancellationToken)
    {
        // Le filtre sur OwnerId garantit qu'on ne supprime qu'une de ses propres recettes (sinon 404).
        var meal = await dbContext.Meals
            .FirstOrDefaultAsync(
                meal => meal.Id == command.Id && meal.OwnerId == currentUser.UserId,
                cancellationToken);

        if (meal is null)
        {
            return Result.Failure(Error.NotFound("Meal.NotFound", "Recette introuvable."));
        }

        dbContext.Meals.Remove(meal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
