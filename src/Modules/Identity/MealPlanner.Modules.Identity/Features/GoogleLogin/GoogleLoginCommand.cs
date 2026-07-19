using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.GoogleLogin;

/// <summary>Authentifie via un <c>id_token</c> Google (obtenu côté client) et renvoie les jetons.</summary>
public sealed record GoogleLoginCommand(string IdToken) : ICommand<Result<AuthResult>>;
