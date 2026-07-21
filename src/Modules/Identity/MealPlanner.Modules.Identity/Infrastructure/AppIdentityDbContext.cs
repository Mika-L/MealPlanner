using MealPlanner.Modules.Identity.Domain;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Identity.Infrastructure;

/// <summary>
/// Contexte EF Core du module Identity. Tables préfixées <c>Identity_</c> (isolation des modules par
/// préfixe de table, un seul fichier SQLite partagé).
/// </summary>
public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public const string TablePrefix = "Identity_";

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserPreferences> Preferences => Set<UserPreferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Renomme les tables Identity par défaut (AspNet*) sous le préfixe du module.
        modelBuilder.Entity<AppUser>().ToTable($"{TablePrefix}Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable($"{TablePrefix}Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable($"{TablePrefix}UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable($"{TablePrefix}UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable($"{TablePrefix}UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable($"{TablePrefix}UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable($"{TablePrefix}RoleClaims");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppIdentityDbContext).Assembly);
    }
}
