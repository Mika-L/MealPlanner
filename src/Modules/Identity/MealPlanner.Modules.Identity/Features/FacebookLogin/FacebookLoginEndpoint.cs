using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.FacebookLogin;

/// <summary>Corps de requête HTTP de la connexion Facebook.</summary>
public sealed record FacebookLoginRequest(string AccessToken);

internal static class FacebookLoginEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/facebook", HandleAsync)
            .AllowAnonymous()
            .WithName("FacebookLogin")
            .WithTags("Auth")
            .WithSummary("Authentifie via un access token Facebook.");
    }

    private static async Task<IResult> HandleAsync(
        FacebookLoginRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["accessToken"] = ["Le jeton Facebook est requis."],
            });
        }

        var result = await dispatcher.SendAsync(new FacebookLoginCommand(request.AccessToken), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
