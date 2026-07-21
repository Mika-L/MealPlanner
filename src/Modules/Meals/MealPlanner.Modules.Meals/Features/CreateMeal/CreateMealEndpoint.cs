using FluentValidation;

using MealPlanner.Modules.Meals.Features;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.CreateMeal;

internal static class CreateMealEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/meals", HandleAsync)
            .RequireAuthorization()
            .WithName("CreateMeal")
            .WithTags("Meals")
            .WithSummary("Ajoute une nouvelle recette au catalogue.");
    }

    private static async Task<IResult> HandleAsync(
        MealWriteRequest request,
        IValidator<CreateMealCommand> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateMealCommand(
            request.Name,
            request.Description,
            request.Seasons ?? [],
            request.Styles ?? [],
            request.PrepTimeMinutes,
            request.Ingredients ?? []);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/meals/{result.Value}", new { id = result.Value })
            : result.Error.ToProblem();
    }
}
