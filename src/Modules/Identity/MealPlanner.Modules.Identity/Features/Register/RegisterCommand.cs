using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.Register;

/// <summary>Crée un compte email/mot de passe et renvoie les jetons d'authentification.</summary>
public sealed record RegisterCommand(string Email, string Password, string? DisplayName)
    : ICommand<Result<AuthResult>>;
