using System.Security.Claims;
using System.Text;

using MealPlanner.Modules.Identity.Domain;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Modules.Identity.Authentication;

internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(AppUser user)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email ?? string.Empty,
                [ClaimTypes.Name] = user.DisplayName ?? user.Email ?? string.Empty,
            },
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, expiresAt);
    }
}
