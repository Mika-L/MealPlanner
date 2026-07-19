using MealPlanner.Modules.Identity.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Modules.Identity.Infrastructure.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable($"{AppIdentityDbContext.TablePrefix}RefreshTokens");

        builder.HasKey(token => token.Id);

        // Clé assignée par le domaine (Guid v7).
        builder.Property(token => token.Id).ValueGeneratedNever();

        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.UserId);
    }
}
