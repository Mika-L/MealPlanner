using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.FacebookLogin;

/// <summary>Authentifie via un access token Facebook (obtenu côté client) et renvoie les jetons.</summary>
public sealed record FacebookLoginCommand(string AccessToken) : ICommand<Result<AuthResult>>;
