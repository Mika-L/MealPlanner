using FluentValidation;

using MealPlanner.Modules.Meals.Domain;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

/// <summary>Corps de requête HTTP mappé vers <see cref="GenerateMealIdeasQuery"/>.</summary>
public sealed record GenerateMealIdeasRequest(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int? Days);

internal static class GenerateMealIdeasEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/meals/ideas", HandleAsync)
            .WithName("GenerateMealIdeas")
            .WithTags("Meals")
            .WithSummary("Génère des idées de repas selon les critères fournis.");
    }

    private static async Task<IResult> HandleAsync(
        GenerateMealIdeasRequest request,
        IValidator<GenerateMealIdeasQuery> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GenerateMealIdeasQuery(
            request.Season,
            request.Styles,
            request.MaxPrepTimeMinutes,
            request.IncludeIngredients,
            request.Days ?? 7);

        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Results.Ok(result.Value);
    }
}
