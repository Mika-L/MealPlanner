namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Identité vérifiée auprès d'un fournisseur externe (Google, Facebook).</summary>
public sealed record ExternalUserInfo(string Provider, string ProviderKey, string Email, string? DisplayName);

/// <summary>Noms des fournisseurs externes (valeur stockée dans <c>AspNetUserLogins</c>).</summary>
public static class ExternalProviders
{
    public const string Google = "Google";
    public const string Facebook = "Facebook";
}
