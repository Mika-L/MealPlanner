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

        // Clé assignée par le domaine (Guid v7) : EF ne doit pas la considérer « générée à l'ajout »,
        // sinon un ingrédient recréé (clé déjà renseignée) est pris pour une entité existante → UPDATE
        // fantôme au lieu d'un INSERT lors du remplacement de la collection.
        builder.Property(ingredient => ingredient.Id).ValueGeneratedNever();

        // Même collation insensible casse/accents que Meal : la recherche par ingrédient (LIKE) reste
        // tolérante côté SQL.
        builder.Property(ingredient => ingredient.Name).HasMaxLength(200).IsRequired()
            .UseCollation(MealsDbContext.SearchCollation);

        builder.HasIndex(ingredient => ingredient.Name);
    }
}
