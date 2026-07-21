namespace MealPlanner.Modules.Identity.Features.Preferences;

/// <summary>Préférences de l'utilisateur exposées au client.</summary>
public sealed record PreferencesResponse(string Theme)
{
    /// <summary>Thème par défaut tant que l'utilisateur n'a rien choisi (aligné sur le front).</summary>
    public const string DefaultTheme = "stone";
}
