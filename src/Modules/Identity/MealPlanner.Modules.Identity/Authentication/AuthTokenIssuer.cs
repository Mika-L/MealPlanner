using System.Security.Cryptography;

using MealPlanner.Modules.Identity.Domain;
using MealPlanner.Modules.Identity.Infrastructure;

using Microsoft.Extensions.Options;

namespace MealPlanner.Modules.Identity.Authentication;

internal sealed class AuthTokenIssuer(
    IJwtTokenService jwtTokenService,
    AppIdentityDbContext dbContext,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IAuthTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public async Task<AuthResult> IssueAsync(AppUser user, CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.CreateAccessToken(user);

        var rawRefreshToken = GenerateRawToken();
        var now = timeProvider.GetUtcNow();
        var refreshToken = new RefreshToken(
            user.Id,
            HashToken(rawRefreshToken),
            now.AddDays(_options.RefreshTokenDays),
            now);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken.Value,
            rawRefreshToken,
            accessToken.ExpiresAt,
            new AuthenticatedUser(user.Id, user.Email ?? string.Empty, user.DisplayName));
    }

    /// <summary>Hache un refresh token en clair pour le comparer/stocker (jamais de clair en base).</summary>
    public static string HashToken(string rawToken)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(hash);
    }

    private static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
