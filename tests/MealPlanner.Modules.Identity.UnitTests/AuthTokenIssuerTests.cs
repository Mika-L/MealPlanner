using MealPlanner.Modules.Identity.Authentication;

namespace MealPlanner.Modules.Identity.UnitTests;

public sealed class AuthTokenIssuerTests
{
    [Fact]
    public void HashToken_should_be_deterministic()
    {
        AuthTokenIssuer.HashToken("un-jeton").Should().Be(AuthTokenIssuer.HashToken("un-jeton"));
    }

    [Fact]
    public void HashToken_should_differ_for_different_inputs_and_never_return_the_clear_value()
    {
        var hash = AuthTokenIssuer.HashToken("un-jeton");

        hash.Should().NotBe(AuthTokenIssuer.HashToken("autre-jeton"));
        hash.Should().NotContain("un-jeton");
        hash.Should().HaveLength(64); // SHA-256 en hexadécimal.
    }
}
