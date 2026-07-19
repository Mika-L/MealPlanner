using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;
using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Identity.Features.RefreshToken;

internal sealed class RefreshTokenHandler(
    AppIdentityDbContext dbContext,
    UserManager<AppUser> userManager,
    IAuthTokenIssuer tokenIssuer,
    TimeProvider timeProvider) : ICommandHandler<RefreshTokenCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var invalid = Error.Unauthorized("Auth.InvalidRefreshToken", "Jeton de rafraîchissement invalide ou expiré.");

        var hash = AuthTokenIssuer.HashToken(command.RefreshToken);
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

        var now = timeProvider.GetUtcNow();
        if (stored is null || !stored.IsActive(now))
        {
            return Result.Failure<AuthResult>(invalid);
        }

        // Rotation : l'ancien jeton est révoqué et remplacé par un nouveau couple.
        stored.Revoke(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Result.Failure<AuthResult>(invalid);
        }

        var result = await tokenIssuer.IssueAsync(user, cancellationToken);
        return Result.Success(result);
    }
}
