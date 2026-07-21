namespace MealPlanner.SharedKernel.Identity;

/// <summary>
/// Utilisateur authentifié de la requête courante. Implémenté par l'hôte (API) à partir du
/// <c>ClaimsPrincipal</c> ; consommé par les modules sans qu'ils connaissent le mécanisme d'auth.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Identifiant de l'utilisateur authentifié.</summary>
    /// <exception cref="InvalidOperationException">Si la requête n'est pas authentifiée.</exception>
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
