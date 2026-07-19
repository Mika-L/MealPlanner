using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.GetCurrentUser;

/// <summary>Renvoie le profil de l'utilisateur authentifié.</summary>
public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<Result<AuthenticatedUser>>;
