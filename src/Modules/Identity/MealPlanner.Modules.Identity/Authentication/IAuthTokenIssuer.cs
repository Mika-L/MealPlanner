using MealPlanner.Modules.Identity.Domain;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>
/// Émet un couple access token + refresh token pour un utilisateur, persiste le refresh token (haché)
/// et renvoie le résultat prêt à être exposé. Point commun à tous les flux d'auth.
/// </summary>
public interface IAuthTokenIssuer
{
    Task<AuthResult> IssueAsync(AppUser user, CancellationToken cancellationToken);
}
