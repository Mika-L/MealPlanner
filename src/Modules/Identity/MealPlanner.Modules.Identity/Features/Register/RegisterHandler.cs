using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Features.Register;

internal sealed class RegisterHandler(
    UserManager<AppUser> userManager,
    UserRegisteredNotifier userRegisteredNotifier,
    IAuthTokenIssuer tokenIssuer) : ICommandHandler<RegisterCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
        {
            return Result.Failure<AuthResult>(
                Error.Conflict("Auth.EmailTaken", "Un compte existe déjà avec cette adresse email."));
        }

        var user = new AppUser
        {
            Id = Guid.CreateVersion7(),
            UserName = command.Email,
            Email = command.Email,
            DisplayName = command.DisplayName,
        };

        var creation = await userManager.CreateAsync(user, command.Password);
        if (!creation.Succeeded)
        {
            var message = string.Join(" ", creation.Errors.Select(error => error.Description));
            return Result.Failure<AuthResult>(Error.Validation("Auth.RegistrationFailed", message));
        }

        await userRegisteredNotifier.NotifyAsync(user.Id, cancellationToken);

        var result = await tokenIssuer.IssueAsync(user, cancellationToken);
        return Result.Success(result);
    }
}
