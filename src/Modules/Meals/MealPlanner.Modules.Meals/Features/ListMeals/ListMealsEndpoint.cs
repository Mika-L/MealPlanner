using FluentValidation;

using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.ListMeals;

internal static class ListMealsEndpoint
{
    private const int DefaultPageSize = 24;

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/meals", HandleAsync)
            .WithName("ListMeals")
            .WithTags("Meals")
            .WithSummary("Liste le catalogue de recettes, filtré et paginé.");
    }

    private static async Task<IResult> HandleAsync(
        IValidator<ListMealsQuery> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        var query = new ListMealsQuery(search, page, pageSize);

        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Results.Ok(result.Value);
    }
}
