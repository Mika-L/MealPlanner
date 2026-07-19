using MealPlanner.Modules.Identity.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Modules.Identity.Infrastructure.Configurations;

internal sealed class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable($"{AppIdentityDbContext.TablePrefix}UserPreferences");

        // Une ligne par utilisateur : la clé primaire est l'identifiant utilisateur.
        builder.HasKey(preferences => preferences.UserId);
        builder.Property(preferences => preferences.UserId).ValueGeneratedNever();

        builder.Property(preferences => preferences.Theme).HasMaxLength(50).IsRequired();
    }
}
