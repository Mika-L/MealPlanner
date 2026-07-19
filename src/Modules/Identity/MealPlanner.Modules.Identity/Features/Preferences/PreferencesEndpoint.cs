using FluentValidation;

using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Identity;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.Preferences;

/// <summary>Corps de requête HTTP de la mise à jour des préférences.</summary>
public sealed record UpdatePreferencesRequest(string Theme);

internal static class PreferencesEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/preferences", GetAsync)
            .RequireAuthorization()
            .WithName("GetPreferences")
            .WithTags("Preferences")
            .WithSummary("Renvoie les préférences de l'utilisateur.");

        endpoints.MapPut("/api/preferences", UpdateAsync)
            .RequireAuthorization()
            .WithName("UpdatePreferences")
            .WithTags("Preferences")
            .WithSummary("Met à jour les préférences de l'utilisateur.");
    }

    private static async Task<IResult> GetAsync(
        ICurrentUser currentUser,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.QueryAsync(new GetPreferencesQuery(currentUser.UserId), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<IResult> UpdateAsync(
        UpdatePreferencesRequest request,
        ICurrentUser currentUser,
        IValidator<UpdatePreferencesCommand> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePreferencesCommand(currentUser.UserId, request.Theme);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(command, cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
    }
}
