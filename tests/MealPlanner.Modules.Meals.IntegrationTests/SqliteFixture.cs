using Microsoft.Data.Sqlite;

namespace MealPlanner.Modules.Meals.IntegrationTests;

/// <summary>Base SQLite sur fichier temporaire, partagée par les tests de la collection.
/// Chaque test recompose un schéma vierge (EnsureDeleted/EnsureCreated).</summary>
public sealed class SqliteFixture : IAsyncLifetime
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"mealplanner-meals-tests-{Guid.NewGuid():N}.db");

    public string ConnectionString => $"Data Source={_databasePath}";

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        // Libère les handles poolés pour pouvoir supprimer le fichier.
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
        return ValueTask.CompletedTask;
    }
}

[CollectionDefinition(nameof(SqliteCollection))]
public sealed class SqliteCollection : ICollectionFixture<SqliteFixture>;
