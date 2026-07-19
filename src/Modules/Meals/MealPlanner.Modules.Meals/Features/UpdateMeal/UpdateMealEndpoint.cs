using FluentValidation;

using MealPlanner.Modules.Meals.Features;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MealPlanner.Modules.Meals.Features.UpdateMeal;

internal static class UpdateMealEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/meals/{id:guid}", HandleAsync)
            .WithName("UpdateMeal")
            .WithTags("Meals")
            .WithSummary("Met à jour une recette existante.");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        MealWriteRequest request,
        IValidator<UpdateMealCommand> validator,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMealCommand(
            id,
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

        return result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();
    }
}
