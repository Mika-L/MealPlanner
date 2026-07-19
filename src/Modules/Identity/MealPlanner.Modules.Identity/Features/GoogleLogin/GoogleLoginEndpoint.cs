using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.GoogleLogin;

/// <summary>Corps de requête HTTP de la connexion Google.</summary>
public sealed record GoogleLoginRequest(string IdToken);

internal static class GoogleLoginEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/google", HandleAsync)
            .AllowAnonymous()
            .WithName("GoogleLogin")
            .WithTags("Auth")
            .WithSummary("Authentifie via un id_token Google.");
    }

    private static async Task<IResult> HandleAsync(
        GoogleLoginRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["idToken"] = ["Le jeton Google est requis."],
            });
        }

        var result = await dispatcher.SendAsync(new GoogleLoginCommand(request.IdToken), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
