using MealPlanner.Modules.Identity;
using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.Modules.Meals;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.MySql;

namespace MealPlanner.Modules.Identity.IntegrationTests;

/// <summary>
/// Démarre un MySQL réel et compose le module Identity (+ Meals, pour le hook de clonage du catalogue)
/// via l'injection de dépendances, comme le fait l'application. Recompose un schéma vierge à la demande.
/// </summary>
public sealed class IdentityModuleFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4")
        .WithDatabase("mealplanner")
        .Build();

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    /// <summary>Construit un provider sur un schéma fraîchement migré. <paramref name="configure"/>
    /// s'applique après les modules et permet de substituer un service (ex. validateur Google).</summary>
    public async Task<ServiceProvider> CreateProviderAsync(
        Action<IServiceCollection>? configure,
        CancellationToken cancellationToken)
    {
        var connectionString = _container.GetConnectionString();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IdentityDb"] = connectionString,
                ["ConnectionStrings:MealsDb"] = connectionString,
                ["Jwt:Issuer"] = "MealPlanner.Tests",
                ["Jwt:Audience"] = "MealPlanner.Tests",
                ["Jwt:SigningKey"] = "integration-tests-signing-key-at-least-32-bytes!!",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICurrentUser>(new StubCurrentUser(Guid.Empty));
        services.AddIdentityModule(configuration);
        services.AddMealsModule(configuration);
        configure?.Invoke(services);

        var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
            var mealsDb = scope.ServiceProvider.GetRequiredService<MealsDbContext>();

            await identityDb.Database.EnsureDeletedAsync(cancellationToken);
            await identityDb.Database.MigrateAsync(cancellationToken);
            await mealsDb.Database.MigrateAsync(cancellationToken);
        }

        return provider;
    }
}

[CollectionDefinition(nameof(IdentityModuleCollection))]
public sealed class IdentityModuleCollection : ICollectionFixture<IdentityModuleFixture>;

internal sealed class StubCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public bool IsAuthenticated => UserId != Guid.Empty;
}
