using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

internal static class ListMealsEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/meals", HandleAsync)
            .WithName("ListMeals")
            .WithTags("Meals")
            .WithSummary("Liste toutes les recettes du catalogue.");
    }

    private static async Task<IResult> HandleAsync(IDispatcher dispatcher, CancellationToken cancellationToken)
    {
        var result = await dispatcher.QueryAsync(new ListMealsQuery(), cancellationToken);

        return Results.Ok(result.Value);
    }
}
