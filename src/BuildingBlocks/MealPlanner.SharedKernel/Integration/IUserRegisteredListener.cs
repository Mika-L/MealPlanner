namespace MealPlanner.SharedKernel.Integration;

/// <summary>
/// Hook in-process déclenché à la création d'un utilisateur. Permet à un module (ex. Meals) de
/// réagir à une inscription gérée par un autre module (Identity) sans dépendance directe entre eux.
/// </summary>
public interface IUserRegisteredListener
{
    Task OnUserRegisteredAsync(Guid userId, CancellationToken cancellationToken);
}
