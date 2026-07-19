using MealPlanner.SharedKernel.Integration;

namespace MealPlanner.Modules.Meals.Infrastructure;

/// <summary>
/// Clone le catalogue de démarrage pour tout nouvel utilisateur : à l'inscription (gérée par le
/// module Identity), l'utilisateur reçoit sa propre copie des recettes de base.
/// </summary>
internal sealed class UserCatalogSeeder(MealsDbContext dbContext) : IUserRegisteredListener
{
    public async Task OnUserRegisteredAsync(Guid userId, CancellationToken cancellationToken)
    {
        dbContext.Meals.AddRange(MealsCatalogTemplate.CreateFor(userId));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
