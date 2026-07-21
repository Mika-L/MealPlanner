using MealPlanner.Modules.Meals.Domain;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Infrastructure;

/// <summary>Contexte EF Core propre au module Meals. Tables préfixées <c>Meals_</c>.</summary>
public sealed class MealsDbContext(DbContextOptions<MealsDbContext> options) : DbContext(options)
{
    public const string TablePrefix = "Meals_";

    /// <summary>Collation SQL Server insensible à la casse et aux accents, appliquée aux colonnes
    /// interrogées par la recherche (nom, description, ingrédient) pour un LIKE tolérant côté SQL.</summary>
    public const string SearchCollation = "Latin1_General_CI_AI";

    public DbSet<Meal> Meals => Set<Meal>();

    public DbSet<MealIngredient> Ingredients => Set<MealIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MealsDbContext).Assembly);
    }
}
