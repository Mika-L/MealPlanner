using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Features.Login;

internal sealed class LoginHandler(
    UserManager<AppUser> userManager,
    IAuthTokenIssuer tokenIssuer) : ICommandHandler<LoginCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        // Message volontairement générique : ne pas révéler si l'email existe.
        var invalidCredentials = Error.Unauthorized("Auth.InvalidCredentials", "Email ou mot de passe incorrect.");

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            return Result.Failure<AuthResult>(invalidCredentials);
        }

        if (!await userManager.CheckPasswordAsync(user, command.Password))
        {
            return Result.Failure<AuthResult>(invalidCredentials);
        }

        var result = await tokenIssuer.IssueAsync(user, cancellationToken);
        return Result.Success(result);
    }
}
