using MealPlanner.Modules.Meals.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Modules.Meals.Infrastructure.Configurations;

internal sealed class MealIngredientConfiguration : IEntityTypeConfiguration<MealIngredient>
{
    public void Configure(EntityTypeBuilder<MealIngredient> builder)
    {
        builder.ToTable($"{MealsDbContext.TablePrefix}Ingredients");

        builder.HasKey(ingredient => ingredient.Id);

        builder.Property(ingredient => ingredient.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(ingredient => ingredient.Name);
    }
}
