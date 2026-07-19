using MealPlanner.Modules.Meals.Features;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.DeleteMeal;

internal static class DeleteMealEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/meals/{id:guid}", HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteMeal")
            .WithTags("Meals")
            .WithSummary("Supprime une recette du catalogue.");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.SendAsync(new DeleteMealCommand(id), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
    }
}
