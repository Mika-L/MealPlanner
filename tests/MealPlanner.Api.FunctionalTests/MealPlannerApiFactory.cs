using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MealPlanner.Api.FunctionalTests;

/// <summary>
/// Fabrique de test in-memory. Fournit une chaîne de connexion factice pour que
/// la composition démarre ; EF n'ouvre aucune connexion tant qu'aucune requête DB n'est faite.
/// Pour des tests bout-en-bout avec DB, brancher un <c>MySqlContainer</c> (Testcontainers).
/// </summary>
public sealed class MealPlannerApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MealsDb"] = "server=localhost;port=3306;database=test;user=root;password=root",
                ["ConnectionStrings:IdentityDb"] = "server=localhost;port=3306;database=test;user=root;password=root",
                ["Jwt:Issuer"] = "MealPlanner.Tests",
                ["Jwt:Audience"] = "MealPlanner.Tests",
                ["Jwt:SigningKey"] = "functional-tests-signing-key-at-least-32-bytes-long!!",
            });
        });
    }
}
