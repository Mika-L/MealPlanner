using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MealPlanner.Modules.Identity.UnitTests;

public sealed class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "MealPlanner.Tests",
        Audience = "MealPlanner.Tests",
        SigningKey = "unit-tests-signing-key-at-least-32-bytes-long!!",
        AccessTokenMinutes = 15,
    };

    [Fact]
    public async Task Should_issue_a_token_carrying_the_user_identity_and_passing_validation()
    {
        var user = new AppUser
        {
            Id = Guid.CreateVersion7(),
            Email = "chef@example.com",
            DisplayName = "Chef",
        };

        var service = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System);
        var accessToken = service.CreateAccessToken(user);

        accessToken.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);

        var validation = await new JsonWebTokenHandler()
            .ValidateTokenAsync(accessToken.Value, JwtBearerParameters.Create(Options));

        validation.IsValid.Should().BeTrue();
        validation.Claims["sub"].Should().Be(user.Id.ToString());
        validation.Claims["email"].Should().Be("chef@example.com");
    }

    [Fact]
    public async Task Should_produce_a_token_rejected_by_a_different_signing_key()
    {
        var user = new AppUser { Id = Guid.CreateVersion7(), Email = "chef@example.com" };

        var service = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System);
        var accessToken = service.CreateAccessToken(user);

        var tamperedOptions = new JwtOptions
        {
            Issuer = Options.Issuer,
            Audience = Options.Audience,
            SigningKey = "a-completely-different-key-of-sufficient-length!!",
        };

        var validation = await new JsonWebTokenHandler()
            .ValidateTokenAsync(accessToken.Value, JwtBearerParameters.Create(tamperedOptions));

        validation.IsValid.Should().BeFalse();
    }
}
