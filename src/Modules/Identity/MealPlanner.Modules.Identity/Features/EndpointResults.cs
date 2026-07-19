using MealPlanner.SharedKernel.Results;

using Microsoft.AspNetCore.Http;

namespace MealPlanner.Modules.Identity.Features;

/// <summary>Traduit une <see cref="Error"/> métier en réponse HTTP <c>ProblemDetails</c>.</summary>
internal static class EndpointResults
{
    public static IResult ToProblem(this Error error) => Results.Problem(
        detail: error.Message,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        });
}
