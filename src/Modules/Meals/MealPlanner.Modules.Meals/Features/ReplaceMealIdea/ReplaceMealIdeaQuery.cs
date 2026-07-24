using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

/// <summary>
/// Remplace une idée d'un planning par une autre recette respectant les mêmes critères (saison, style,
/// temps) et sans réutiliser les ingrédients du frigo déjà consommés par les repas conservés
/// (règle « un ingrédient = un repas »). La recette écartée et les repas conservés ne sont jamais repiochés.
/// <para>
/// <see cref="SeenMealIds"/> liste les recettes déjà proposées sur l'ensemble du planning (tous jours
/// confondus, idées initiales comprises) : on en pioche une nouvelle à chaque remplacement pour garantir
/// « une autre à chaque fois », et une recette écartée ne réapparaît pas sur un autre jour. Une fois
/// toutes les alternatives épuisées, le cycle repart depuis le début — sans jamais reproposer la recette
/// actuellement affichée (<see cref="ReplacedMealId"/>).
/// </para>
/// </summary>
public sealed record ReplaceMealIdeaQuery(
    Season? Season,
    MealStyle? Styles,
    int? MaxPrepTimeMinutes,
    IReadOnlyList<string>? IncludeIngredients,
    int Day,
    Guid ReplacedMealId,
    IReadOnlyList<Guid> KeptMealIds,
    IReadOnlyList<Guid> SeenMealIds) : IQuery<Result<ReplaceMealIdeaResponse>>;

public sealed record ReplaceMealIdeaResponse(PlannedMeal Meal);
