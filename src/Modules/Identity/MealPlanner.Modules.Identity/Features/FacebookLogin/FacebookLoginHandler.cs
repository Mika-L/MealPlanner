using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.FacebookLogin;

internal sealed class FacebookLoginHandler(
    IFacebookTokenValidator facebookTokenValidator,
    ExternalLoginService externalLoginService,
    IAuthTokenIssuer tokenIssuer) : ICommandHandler<FacebookLoginCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> HandleAsync(FacebookLoginCommand command, CancellationToken cancellationToken)
    {
        var info = await facebookTokenValidator.ValidateAsync(command.AccessToken, cancellationToken);
        if (info is null)
        {
            return Result.Failure<AuthResult>(
                Error.Unauthorized("Auth.InvalidFacebookToken", "Jeton Facebook invalide."));
        }

        var user = await externalLoginService.FindOrCreateAsync(info, cancellationToken);

        var result = await tokenIssuer.IssueAsync(user, cancellationToken);
        return Result.Success(result);
    }
}
