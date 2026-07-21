using MealPlanner.Modules.Identity.Domain;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>
/// Rattache une identité externe (Google, Facebook) à un <see cref="AppUser"/> : réutilise le compte
/// déjà lié, sinon celui du même email, sinon en crée un ; notifie l'inscription en cas de création.
/// </summary>
internal sealed class ExternalLoginService(
    UserManager<AppUser> userManager,
    UserRegisteredNotifier userRegisteredNotifier)
{
    public async Task<AppUser> FindOrCreateAsync(ExternalUserInfo info, CancellationToken cancellationToken)
    {
        var linked = await userManager.FindByLoginAsync(info.Provider, info.ProviderKey);
        if (linked is not null)
        {
            return linked;
        }

        var existing = await userManager.FindByEmailAsync(info.Email);
        if (existing is not null)
        {
            // Compte email/mdp déjà présent : on y rattache le login externe.
            await userManager.AddLoginAsync(existing, new UserLoginInfo(info.Provider, info.ProviderKey, info.Provider));
            return existing;
        }

        var user = new AppUser
        {
            Id = Guid.CreateVersion7(),
            UserName = info.Email,
            Email = info.Email,
            EmailConfirmed = true,
            DisplayName = info.DisplayName,
        };

        await userManager.CreateAsync(user);
        await userManager.AddLoginAsync(user, new UserLoginInfo(info.Provider, info.ProviderKey, info.Provider));
        await userRegisteredNotifier.NotifyAsync(user.Id, cancellationToken);

        return user;
    }
}
