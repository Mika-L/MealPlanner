using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Features.GetCurrentUser;

internal sealed class GetCurrentUserHandler(UserManager<AppUser> userManager)
    : IQueryHandler<GetCurrentUserQuery, Result<AuthenticatedUser>>
{
    public async Task<Result<AuthenticatedUser>> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user is null)
        {
            return Result.Failure<AuthenticatedUser>(Error.NotFound("Auth.UserNotFound", "Utilisateur introuvable."));
        }

        return Result.Success(new AuthenticatedUser(user.Id, user.Email ?? string.Empty, user.DisplayName));
    }
}
