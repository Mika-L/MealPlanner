using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MealPlanner.Modules.Meals.Infrastructure;

/// <summary>
/// Utilisé uniquement par les outils EF Core (<c>dotnet ef migrations</c>).
/// La chaîne de connexion réelle vient de la configuration de l'API au runtime.
/// </summary>
public sealed class MealsDbContextFactory : IDesignTimeDbContextFactory<MealsDbContext>
{
    public MealsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MEALS_DB_CONNECTION")
            ?? "Server=localhost,1433;Database=MealPlanner;User Id=sa;Password=LocalDev_p4ssw0rd;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<MealsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MealsDbContext(options);
    }
}
