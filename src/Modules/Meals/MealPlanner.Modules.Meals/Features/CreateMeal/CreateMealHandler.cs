using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.CreateMeal;

internal sealed class CreateMealHandler(MealsDbContext dbContext, ICurrentUser currentUser)
    : ICommandHandler<CreateMealCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMealCommand command, CancellationToken cancellationToken)
    {
        var meal = new Meal(
            currentUser.UserId,
            command.Name,
            command.Description,
            FlagEnum.Combine(command.Seasons),
            FlagEnum.Combine(command.Styles),
            command.PrepTimeMinutes);

        meal.ReplaceIngredients(command.Ingredients);

        dbContext.Meals.Add(meal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(meal.Id);
    }
}
