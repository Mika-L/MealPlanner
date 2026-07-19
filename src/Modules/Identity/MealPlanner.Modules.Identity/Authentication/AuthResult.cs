namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Jetons émis + profil de l'utilisateur authentifié. Renvoyé par tous les flux d'auth.</summary>
public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    AuthenticatedUser User);

/// <summary>Profil minimal de l'utilisateur exposé au client.</summary>
public sealed record AuthenticatedUser(Guid Id, string Email, string? DisplayName);
