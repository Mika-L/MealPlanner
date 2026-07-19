namespace MealPlanner.Modules.Identity.Domain;

/// <summary>
/// Jeton de rafraîchissement stocké haché (jamais en clair). Un seul jeton actif par émission ;
/// la rotation révoque l'ancien et en crée un nouveau.
/// </summary>
public sealed class RefreshToken
{
    // EF Core
    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt = now;
}
