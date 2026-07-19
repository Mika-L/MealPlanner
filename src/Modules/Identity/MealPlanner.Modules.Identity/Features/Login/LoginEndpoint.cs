using FluentValidation;

using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Identity.Features.Login;

/// <summary>Corps de requête HTTP de la connexion.</summary>
public sealed record LoginRequest(string Email, string Password);

internal static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", HandleAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithTags("Auth")
            .WithSummary("Authentifie un compte email/mot de passe.");
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        IValidator<LoginCommand> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
