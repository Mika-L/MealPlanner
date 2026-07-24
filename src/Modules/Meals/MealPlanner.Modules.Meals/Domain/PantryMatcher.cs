namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Un ingrédient du frigo, sous sa forme saisie et sa forme normalisée (voir <see cref="SearchText"/>).</summary>
public sealed record PantryTerm(string Original, string Normalized);

/// <summary>
/// Correspondance souple entre les ingrédients disponibles (« frigo ») et ceux d'un repas.
/// Cette logique — normalisation, dédup, matching par sous-chaîne — ne s'exprime pas en SQL : elle est
/// partagée par la génération d'idées et le remplacement d'une idée pour rester une source unique de vérité.
/// </summary>
public static class PantryMatcher
{
    /// <summary>Normalise les ingrédients disponibles en termes distincts, en écartant les entrées vides.</summary>
    public static IReadOnlyList<PantryTerm> BuildPantry(IEnumerable<string> availableIngredients) =>
        availableIngredients
            .Select(term => new PantryTerm(term.Trim(), SearchText.Normalize(term)))
            .Where(term => term.Normalized.Length > 0)
            .DistinctBy(term => term.Normalized)
            .ToList();

    /// <summary>Termes du frigo qu'un repas utilise : un ingrédient du repas <em>contient</em> le terme normalisé.</summary>
    public static IReadOnlyList<PantryTerm> Match(Meal meal, IReadOnlyList<PantryTerm> pantry) =>
        pantry
            .Where(term => meal.Ingredients.Any(ingredient =>
                SearchText.Normalize(ingredient.Name).Contains(term.Normalized)))
            .ToList();
}
