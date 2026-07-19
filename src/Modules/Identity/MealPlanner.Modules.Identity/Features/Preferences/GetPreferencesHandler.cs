using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Identity.Features.Preferences;

internal sealed class GetPreferencesHandler(AppIdentityDbContext dbContext)
    : IQueryHandler<GetPreferencesQuery, Result<PreferencesResponse>>
{
    public async Task<Result<PreferencesResponse>> HandleAsync(
        GetPreferencesQuery query,
        CancellationToken cancellationToken)
    {
        var preferences = await dbContext.Preferences
            .FirstOrDefaultAsync(entry => entry.UserId == query.UserId, cancellationToken);

        var theme = preferences?.Theme ?? PreferencesResponse.DefaultTheme;
        return Result.Success(new PreferencesResponse(theme));
    }
}
