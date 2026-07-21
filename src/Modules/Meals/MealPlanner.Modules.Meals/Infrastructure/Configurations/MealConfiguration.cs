using MealPlanner.Modules.Meals.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Modules.Meals.Infrastructure.Configurations;

internal sealed class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable($"{MealsDbContext.TablePrefix}Meals");

        builder.HasKey(meal => meal.Id);

        // Clé assignée par le domaine (Guid v7), voir MealIngredientConfiguration.
        builder.Property(meal => meal.Id).ValueGeneratedNever();

        builder.Property(meal => meal.OwnerId).IsRequired();
        builder.HasIndex(meal => meal.OwnerId);

        // Collation insensible à la casse ET aux accents : la recherche (LIKE) matche "gruyere"
        // avec "Gruyère" directement en SQL, sans normalisation applicative.
        builder.Property(meal => meal.Name).HasMaxLength(200).IsRequired().UseCollation(MealsDbContext.SearchCollation);
        builder.Property(meal => meal.Description).HasMaxLength(2000).IsRequired().UseCollation(MealsDbContext.SearchCollation);
        builder.Property(meal => meal.Seasons).HasConversion<int>();
        builder.Property(meal => meal.Styles).HasConversion<int>();
        builder.Property(meal => meal.PrepTimeMinutes);

        builder.HasMany(meal => meal.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(meal => meal.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
