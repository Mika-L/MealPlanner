using FluentValidation;

using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.Register;

/// <summary>Corps de requête HTTP de l'inscription.</summary>
public sealed record RegisterRequest(string Email, string Password, string? DisplayName);

internal static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/register", HandleAsync)
            .AllowAnonymous()
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Crée un compte email/mot de passe.");
    }

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        IValidator<RegisterCommand> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.DisplayName);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
