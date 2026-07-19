using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Identity.Features.Preferences;

/// <summary>Renvoie les préférences de l'utilisateur (valeurs par défaut si aucune n'est enregistrée).</summary>
public sealed record GetPreferencesQuery(Guid UserId) : IQuery<Result<PreferencesResponse>>;
