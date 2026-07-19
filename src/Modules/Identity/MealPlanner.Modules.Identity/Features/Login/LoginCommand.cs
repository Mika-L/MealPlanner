using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.Login;

/// <summary>Authentifie un compte email/mot de passe et renvoie les jetons.</summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<Result<AuthResult>>;
