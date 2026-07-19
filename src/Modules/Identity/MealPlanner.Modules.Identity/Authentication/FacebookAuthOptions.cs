namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Configuration Facebook (section <c>Authentication:Facebook</c>).</summary>
public sealed class FacebookAuthOptions
{
    public const string SectionName = "Authentication:Facebook";

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;
}
