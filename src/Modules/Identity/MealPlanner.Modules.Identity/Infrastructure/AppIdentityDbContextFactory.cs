using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MealPlanner.Modules.Identity.Infrastructure;

/// <summary>
/// Utilisé uniquement par les outils EF Core (<c>dotnet ef migrations</c>).
/// La chaîne de connexion réelle vient de la configuration de l'API au runtime.
/// </summary>
public sealed class AppIdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? "server=localhost;port=3306;database=mealplanner;user=root;password=root";

        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseMySQL(connectionString)
            .Options;

        return new AppIdentityDbContext(options);
    }
}
