using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.RefreshToken;

/// <summary>Corps de requête HTTP du rafraîchissement.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);

internal static class RefreshTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/refresh", HandleAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithTags("Auth")
            .WithSummary("Échange un refresh token contre un nouveau couple de jetons.");
    }

    private static async Task<IResult> HandleAsync(
        RefreshTokenRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["refreshToken"] = ["Le jeton de rafraîchissement est requis."],
            });
        }

        var result = await dispatcher.SendAsync(new RefreshTokenCommand(request.RefreshToken), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
