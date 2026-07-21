using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace MealPlanner.Api.FunctionalTests;

/// <summary>
/// Fabrique de test in-memory. L'API démarre en Development et migre au démarrage : on la pointe
/// vers un fichier SQLite temporaire, supprimé à la libération de la fabrique.
/// </summary>
public sealed class MealPlannerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"mealplanner-functional-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = $"Data Source={_databasePath}";

        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MealsDb"] = connectionString,
                ["ConnectionStrings:IdentityDb"] = connectionString,
                ["Jwt:Issuer"] = "MealPlanner.Tests",
                ["Jwt:Audience"] = "MealPlanner.Tests",
                ["Jwt:SigningKey"] = "functional-tests-signing-key-at-least-32-bytes-long!!",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_databasePath);
        }
    }
}
