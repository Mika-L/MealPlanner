namespace MealPlanner.Modules.Identity.Domain;

/// <summary>Préférences propres à un utilisateur (une ligne par utilisateur). Portées côté serveur.</summary>
public sealed class UserPreferences
{
    // EF Core
    private UserPreferences()
    {
    }

    public UserPreferences(Guid userId, string theme)
    {
        UserId = userId;
        Theme = theme;
    }

    public Guid UserId { get; private set; }

    public string Theme { get; private set; } = string.Empty;

    public void SetTheme(string theme) => Theme = theme;
}
