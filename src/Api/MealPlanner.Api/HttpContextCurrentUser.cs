using MealPlanner.SharedKernel.Identity;

namespace MealPlanner.Api;

/// <summary>
/// Implémentation de <see cref="ICurrentUser"/> côté hôte : lit l'identité depuis le
/// <c>ClaimsPrincipal</c> de la requête (claim <c>sub</c> du JWT).
/// </summary>
internal sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string SubjectClaim = "sub";

    public Guid UserId => Guid.TryParse(Subject, out var userId)
        ? userId
        : throw new InvalidOperationException("Aucun utilisateur authentifié dans le contexte courant.");

    public bool IsAuthenticated => Guid.TryParse(Subject, out _);

    private string? Subject => httpContextAccessor.HttpContext?.User.FindFirst(SubjectClaim)?.Value;
}
