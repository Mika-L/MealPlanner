using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.UpdateMeal;

internal sealed class UpdateMealHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<UpdateMealCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMealCommand command, CancellationToken cancellationToken)
    {
        // Le filtre sur OwnerId garantit qu'on ne modifie qu'une de ses propres recettes (sinon 404).
        var meal = await dbContext.Meals
            .Include(meal => meal.Ingredients)
            .FirstOrDefaultAsync(
                meal => meal.Id == command.Id && meal.OwnerId == currentUser.UserId,
                cancellationToken);

        if (meal is null)
        {
            return Result.Failure(Error.NotFound("Meal.NotFound", "Recette introuvable."));
        }

        meal.Update(
            command.Name,
            command.Description,
            FlagEnum.Combine(command.Seasons),
            FlagEnum.Combine(command.Styles),
            command.PrepTimeMinutes);

        meal.ReplaceIngredients(command.Ingredients);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
