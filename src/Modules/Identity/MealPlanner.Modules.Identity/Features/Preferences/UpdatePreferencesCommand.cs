using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.Preferences;

/// <summary>Met à jour les préférences de l'utilisateur (création si absentes).</summary>
public sealed record UpdatePreferencesCommand(Guid UserId, string Theme) : ICommand<Result>;
