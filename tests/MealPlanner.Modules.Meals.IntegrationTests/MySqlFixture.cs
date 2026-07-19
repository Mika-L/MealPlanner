using Testcontainers.MySql;

namespace MealPlanner.Modules.Meals.IntegrationTests;

/// <summary>Démarre un conteneur MySQL réel, partagé par les tests de la collection.</summary>
public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4")
        .WithDatabase("mealplanner")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}

[CollectionDefinition(nameof(MySqlCollection))]
public sealed class MySqlCollection : ICollectionFixture<MySqlFixture>;
