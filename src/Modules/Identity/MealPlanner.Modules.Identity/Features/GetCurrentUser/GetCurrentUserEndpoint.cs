using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.GetCurrentUser;

internal static class GetCurrentUserEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/auth/me", HandleAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithTags("Auth")
            .WithSummary("Renvoie le profil de l'utilisateur authentifié.");
    }

    private static async Task<IResult> HandleAsync(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.QueryAsync(new GetCurrentUserQuery(currentUser.UserId), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
