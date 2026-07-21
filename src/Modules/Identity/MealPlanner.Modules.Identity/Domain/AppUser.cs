using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Domain;

/// <summary>
/// Utilisateur de l'application. Étend <see cref="IdentityUser{TKey}"/> : email/mot de passe et
/// logins externes (Google, Facebook) sont gérés nativement par ASP.NET Core Identity.
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
}
