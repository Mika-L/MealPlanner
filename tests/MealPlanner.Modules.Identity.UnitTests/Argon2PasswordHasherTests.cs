using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.UnitTests;

public sealed class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher = new();
    private readonly AppUser _user = new();

    [Fact]
    public void VerifyHashedPassword_should_succeed_for_the_original_password()
    {
        var hash = _hasher.HashPassword(_user, "correct horse battery staple");

        _hasher.VerifyHashedPassword(_user, hash, "correct horse battery staple")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_should_fail_for_a_wrong_password()
    {
        var hash = _hasher.HashPassword(_user, "correct horse battery staple");

        _hasher.VerifyHashedPassword(_user, hash, "wrong horse battery staple")
            .Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void HashPassword_should_use_a_random_salt_and_never_expose_the_clear_password()
    {
        var first = _hasher.HashPassword(_user, "s3cret spaces éàü");
        var second = _hasher.HashPassword(_user, "s3cret spaces éàü");

        first.Should().StartWith("$argon2id$");
        first.Should().NotContain("s3cret");
        first.Should().NotBe(second); // sel aléatoire → condensats distincts…
        _hasher.VerifyHashedPassword(_user, second, "s3cret spaces éàü")
            .Should().Be(PasswordVerificationResult.Success); // …mais tous deux vérifiables.
    }

    [Theory]
    [InlineData("")]
    [InlineData("pas-un-hash-argon2")]
    [InlineData("$argon2id$v=19$m=19456,t=2,p=1$corrompu")]
    public void VerifyHashedPassword_should_fail_gracefully_on_a_malformed_hash(string storedHash)
    {
        _hasher.VerifyHashedPassword(_user, storedHash, "peu importe")
            .Should().Be(PasswordVerificationResult.Failed);
    }
}
