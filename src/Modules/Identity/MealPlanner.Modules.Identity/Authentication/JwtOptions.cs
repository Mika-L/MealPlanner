namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>Paramètres d'émission et de validation des JWT (section de configuration <c>Jwt</c>).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Clé symétrique de signature. À fournir via user-secrets / variable d'environnement.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
