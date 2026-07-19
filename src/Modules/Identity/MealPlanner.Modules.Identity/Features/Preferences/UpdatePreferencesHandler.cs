using MealPlanner.Modules.Identity.Domain;
using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Identity.Features.Preferences;

internal sealed class UpdatePreferencesHandler(AppIdentityDbContext dbContext)
    : ICommandHandler<UpdatePreferencesCommand, Result>
{
    public async Task<Result> HandleAsync(UpdatePreferencesCommand command, CancellationToken cancellationToken)
    {
        var preferences = await dbContext.Preferences
            .FirstOrDefaultAsync(entry => entry.UserId == command.UserId, cancellationToken);

        if (preferences is null)
        {
            dbContext.Preferences.Add(new UserPreferences(command.UserId, command.Theme));
        }
        else
        {
            preferences.SetTheme(command.Theme);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
