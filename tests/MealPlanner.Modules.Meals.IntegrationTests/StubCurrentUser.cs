using MealPlanner.SharedKernel.Identity;

namespace MealPlanner.Modules.Meals.IntegrationTests;

/// <summary>Utilisateur courant figé pour les tests, sans dépendance à HTTP.</summary>
internal sealed class StubCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public bool IsAuthenticated => true;
}
