namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Valide un access token Facebook et en extrait l'identité, ou <c>null</c> si invalide.</summary>
public interface IFacebookTokenValidator
{
    Task<ExternalUserInfo?> ValidateAsync(string accessToken, CancellationToken cancellationToken);
}
