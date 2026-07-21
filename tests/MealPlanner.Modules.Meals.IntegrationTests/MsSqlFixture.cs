using Microsoft.Data.SqlClient;

using Testcontainers.MsSql;

namespace MealPlanner.Modules.Meals.IntegrationTests;

/// <summary>Conteneur SQL Server éphémère, partagé par les tests de la collection. Chaque test recompose
/// un schéma vierge (EnsureDeleted/EnsureCreated) sur la base dédiée <c>MealPlannerTests</c> — les tests
/// d'une même collection s'exécutent en série, ce partage est donc sûr.</summary>
public sealed class MsSqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);

        // Le conteneur pointe master par défaut : on cible une base dédiée pour pouvoir la drop/recréer.
        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "MealPlannerTests",
        }.ConnectionString;
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(MsSqlCollection))]
public sealed class MsSqlCollection : ICollectionFixture<MsSqlFixture>;
