using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Features.GoogleLogin;
using MealPlanner.Modules.Identity.Features.Login;
using MealPlanner.Modules.Identity.Features.RefreshToken;
using MealPlanner.Modules.Identity.Features.Register;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Modules.Identity.IntegrationTests;

[Collection(nameof(IdentityModuleCollection))]
public sealed class AuthFlowTests(IdentityModuleFixture fixture)
{
    [Fact]
    public async Task Register_should_issue_tokens_and_clone_the_starter_catalog_for_the_new_user()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var provider = await fixture.CreateProviderAsync(null, cancellationToken);

        AuthResult registration;
        await using (var scope = provider.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
                .SendAsync(new RegisterCommand("chef@example.com", "Password1!", "Chef"), cancellationToken);

            result.IsSuccess.Should().BeTrue();
            registration = result.Value;
        }

        registration.AccessToken.Should().NotBeEmpty();
        registration.RefreshToken.Should().NotBeEmpty();
        registration.User.Email.Should().Be("chef@example.com");

        // Le catalogue de démarrage a été cloné pour ce nouvel utilisateur.
        await using var verifyScope = provider.CreateAsyncScope();
        var mealsDb = verifyScope.ServiceProvider.GetRequiredService<MealsDbContext>();
        var ownedMeals = await mealsDb.Meals
            .CountAsync(meal => meal.OwnerId == registration.User.Id, cancellationToken);
        ownedMeals.Should().Be(15);
    }

    [Fact]
    public async Task Register_should_fail_when_the_email_is_already_taken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var provider = await fixture.CreateProviderAsync(null, cancellationToken);

        await RegisterAsync(provider, "dup@example.com", cancellationToken);

        await using var scope = provider.CreateAsyncScope();
        var second = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
            .SendAsync(new RegisterCommand("dup@example.com", "Password1!", null), cancellationToken);

        second.IsFailure.Should().BeTrue();
        second.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Register_should_accept_a_lowercase_passphrase_without_composition_rules()
    {
        // NIST : aucune exigence majuscule/chiffre/spécial ne doit bloquer une phrase de passe longue.
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var provider = await fixture.CreateProviderAsync(null, cancellationToken);

        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
            .SendAsync(
                new RegisterCommand("passphrase@example.com", "correct horse battery staple", null),
                cancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Login_should_succeed_with_the_right_password_and_fail_otherwise()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var provider = await fixture.CreateProviderAsync(null, cancellationToken);

        await RegisterAsync(provider, "login@example.com", cancellationToken);

        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

        var success = await dispatcher.SendAsync(
            new LoginCommand("login@example.com", "Password1!"), cancellationToken);
        success.IsSuccess.Should().BeTrue();
        success.Value.User.Email.Should().Be("login@example.com");

        var failure = await dispatcher.SendAsync(
            new LoginCommand("login@example.com", "wrong-password"), cancellationToken);
        failure.IsFailure.Should().BeTrue();
        failure.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Refresh_should_rotate_the_token_and_reject_the_previous_one()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var provider = await fixture.CreateProviderAsync(null, cancellationToken);

        var registration = await RegisterAsync(provider, "refresh@example.com", cancellationToken);

        AuthResult rotated;
        await using (var scope = provider.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
                .SendAsync(new RefreshTokenCommand(registration.RefreshToken), cancellationToken);
            result.IsSuccess.Should().BeTrue();
            rotated = result.Value;
        }

        rotated.RefreshToken.Should().NotBe(registration.RefreshToken);

        // L'ancien refresh token est révoqué : il ne doit plus être accepté.
        await using var reuseScope = provider.CreateAsyncScope();
        var reuse = await reuseScope.ServiceProvider.GetRequiredService<IDispatcher>()
            .SendAsync(new RefreshTokenCommand(registration.RefreshToken), cancellationToken);
        reuse.IsFailure.Should().BeTrue();
        reuse.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Google_login_should_create_and_provision_a_user_from_a_verified_identity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var googleValidator = Substitute.For<IGoogleTokenValidator>();
        googleValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ExternalUserInfo(ExternalProviders.Google, "google-123", "gmail@example.com", "Gmail User"));

        await using var provider = await fixture.CreateProviderAsync(
            services => services.AddScoped(_ => googleValidator),
            cancellationToken);

        AuthResult login;
        await using (var scope = provider.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
                .SendAsync(new GoogleLoginCommand("any-id-token"), cancellationToken);
            result.IsSuccess.Should().BeTrue();
            login = result.Value;
        }

        login.User.Email.Should().Be("gmail@example.com");

        // Comme pour une inscription classique, le catalogue de démarrage est cloné.
        await using var verifyScope = provider.CreateAsyncScope();
        var mealsDb = verifyScope.ServiceProvider.GetRequiredService<MealsDbContext>();
        var ownedMeals = await mealsDb.Meals
            .CountAsync(meal => meal.OwnerId == login.User.Id, cancellationToken);
        ownedMeals.Should().Be(15);
    }

    private static async Task<AuthResult> RegisterAsync(
        ServiceProvider provider,
        string email,
        CancellationToken cancellationToken)
    {
        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IDispatcher>()
            .SendAsync(new RegisterCommand(email, "Password1!", null), cancellationToken);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
