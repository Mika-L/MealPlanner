using MealPlanner.SharedKernel.Integration;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Notifie les modules abonnés qu'un utilisateur vient d'être créé (ex. clonage du catalogue).</summary>
internal sealed class UserRegisteredNotifier(IEnumerable<IUserRegisteredListener> listeners)
{
    public async Task NotifyAsync(Guid userId, CancellationToken cancellationToken)
    {
        foreach (var listener in listeners)
        {
            await listener.OnUserRegisteredAsync(userId, cancellationToken);
        }
    }
}
