using FluentValidation;

using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

/// <summary>Corps de requête HTTP mappé vers <see cref="ReplaceMealIdeaQuery"/>.</summary>
public sealed record ReplaceMealIdeaRequest(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int Day,
    Guid ReplacedMealId,
    IReadOnlyList<Guid>? KeptMealIds);

internal static class ReplaceMealIdeaEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/meals/ideas/replace", HandleAsync)
            .RequireAuthorization()
            .WithName("ReplaceMealIdea")
            .WithTags("Meals")
            .WithSummary("Remplace une idée d'un planning par une autre recette respectant les mêmes critères.");
    }

    private static async Task<IResult> HandleAsync(
        ReplaceMealIdeaRequest request,
        IValidator<ReplaceMealIdeaQuery> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new ReplaceMealIdeaQuery(
            request.Season,
            request.Styles,
            request.MaxPrepTimeMinutes,
            request.IncludeIngredients,
            request.Day,
            request.ReplacedMealId,
            request.KeptMealIds ?? []);

        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
