using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.GoogleLogin;

internal sealed class GoogleLoginHandler(
    IGoogleTokenValidator googleTokenValidator,
    ExternalLoginService externalLoginService,
    IAuthTokenIssuer tokenIssuer) : ICommandHandler<GoogleLoginCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> HandleAsync(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        var info = await googleTokenValidator.ValidateAsync(command.IdToken, cancellationToken);
        if (info is null)
        {
            return Result.Failure<AuthResult>(
                Error.Unauthorized("Auth.InvalidGoogleToken", "Jeton Google invalide."));
        }

        var user = await externalLoginService.FindOrCreateAsync(info, cancellationToken);

        var result = await tokenIssuer.IssueAsync(user, cancellationToken);
        return Result.Success(result);
    }
}
