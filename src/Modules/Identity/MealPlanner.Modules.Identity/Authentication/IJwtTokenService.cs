using MealPlanner.Modules.Identity.Domain;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Émet les access tokens JWT signés par l'application.</summary>
public interface IJwtTokenService
{
    AccessToken CreateAccessToken(AppUser user);
}

/// <summary>Access token signé et sa date d'expiration.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
