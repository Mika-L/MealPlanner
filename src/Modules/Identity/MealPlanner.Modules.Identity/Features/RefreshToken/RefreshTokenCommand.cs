using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.RefreshToken;

/// <summary>Échange un refresh token valide contre un nouveau couple de jetons (rotation).</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<Result<AuthResult>>;
