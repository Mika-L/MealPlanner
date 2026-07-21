using MealPlanner.Modules.Identity;
using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.Modules.Meals;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Identity;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.MsSql;

namespace MealPlanner.Modules.Identity.IntegrationTests;

/// <summary>
/// Compose le module Identity (+ Meals, pour le hook de clonage du catalogue) via l'injection de
/// dépendances, comme le fait l'application, sur un conteneur SQL Server éphémère partagé par la
/// collection. Recompose un schéma vierge à la demande.
/// </summary>
public sealed class IdentityModuleFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);

        // Le conteneur pointe master par défaut : on cible une base dédiée pour pouvoir la drop/recréer.
        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "MealPlannerTests",
        }.ConnectionString;
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    /// <summary>Construit un provider sur un schéma fraîchement migré. <paramref name="configure"/>
    /// s'applique après les modules et permet de substituer un service (ex. validateur Google).</summary>
    public async Task<ServiceProvider> CreateProviderAsync(
        Action<IServiceCollection>? configure,
        CancellationToken cancellationToken)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IdentityDb"] = _connectionString,
                ["ConnectionStrings:MealsDb"] = _connectionString,
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
