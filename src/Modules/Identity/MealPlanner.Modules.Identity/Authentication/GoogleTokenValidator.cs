using Google.Apis.Auth;

using Microsoft.Extensions.Options;

namespace MealPlanner.Modules.Identity.Authentication;

internal sealed class GoogleTokenValidator(IOptions<GoogleAuthOptions> options) : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options = options.Value;

    public async Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_options.ClientId],
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (string.IsNullOrEmpty(payload.Email))
            {
                return null;
            }

            return new ExternalUserInfo(ExternalProviders.Google, payload.Subject, payload.Email, payload.Name);
        }
        catch (InvalidJwtException)
        {
            // Signature, audience ou expiration invalide : identité non prouvée.
            return null;
        }
    }
}
