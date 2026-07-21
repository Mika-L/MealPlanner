using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Testcontainers.MsSql;

namespace MealPlanner.Api.FunctionalTests;

/// <summary>
/// Fabrique de test in-memory. L'API démarre en Development et migre au démarrage : on la pointe vers
/// un conteneur SQL Server éphémère, partagé par la collection (une seule instance émulée). Le conteneur
/// est démarré avant la construction de l'hôte (IAsyncLifetime).
/// </summary>
public sealed class MealPlannerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private string _connectionString = string.Empty;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "MealPlannerFunctionalTests",
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MealsDb"] = _connectionString,
                ["ConnectionStrings:IdentityDb"] = _connectionString,
                ["Jwt:Issuer"] = "MealPlanner.Tests",
                ["Jwt:Audience"] = "MealPlanner.Tests",
                ["Jwt:SigningKey"] = "functional-tests-signing-key-at-least-32-bytes-long!!",
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(MealPlannerApiCollection))]
public sealed class MealPlannerApiCollection : ICollectionFixture<MealPlannerApiFactory>;
