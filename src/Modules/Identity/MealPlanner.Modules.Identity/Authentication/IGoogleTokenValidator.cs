namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Valide un <c>id_token</c> Google et en extrait l'identité, ou <c>null</c> si invalide.</summary>
public interface IGoogleTokenValidator
{
    Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
