namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Configuration Google (section <c>Authentication:Google</c>).</summary>
public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    /// <summary>ID client OAuth Google : audience attendue des <c>id_token</c> reçus du front.</summary>
    public string ClientId { get; set; } = string.Empty;
}
