using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>
/// Valide un access token utilisateur Facebook via l'API Graph : vérifie sa validité et son
/// appartenance à notre application (<c>/debug_token</c>), puis récupère le profil (<c>/me</c>).
/// </summary>
internal sealed class FacebookTokenValidator(IHttpClientFactory httpClientFactory, IOptions<FacebookAuthOptions> options)
    : IFacebookTokenValidator
{
    public const string HttpClientName = "Facebook";

    private readonly FacebookAuthOptions _options = options.Value;

    public async Task<ExternalUserInfo?> ValidateAsync(string accessToken, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var appToken = $"{_options.AppId}|{_options.AppSecret}";

        var debug = await client.GetFromJsonAsync<DebugTokenResponse>(
            $"debug_token?input_token={Uri.EscapeDataString(accessToken)}&access_token={Uri.EscapeDataString(appToken)}",
            cancellationToken);

        if (debug?.Data is not { IsValid: true } data || data.AppId != _options.AppId)
        {
            return null;
        }

        var profile = await client.GetFromJsonAsync<ProfileResponse>(
            $"me?fields=id,name,email&access_token={Uri.EscapeDataString(accessToken)}",
            cancellationToken);

        if (profile is null || string.IsNullOrEmpty(profile.Id) || string.IsNullOrEmpty(profile.Email))
        {
            return null;
        }

        return new ExternalUserInfo(ExternalProviders.Facebook, profile.Id, profile.Email, profile.Name);
    }

    private sealed record DebugTokenResponse([property: JsonPropertyName("data")] DebugTokenData? Data);

    private sealed record DebugTokenData(
        [property: JsonPropertyName("app_id")] string? AppId,
        [property: JsonPropertyName("is_valid")] bool IsValid);

    private sealed record ProfileResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email);
}
