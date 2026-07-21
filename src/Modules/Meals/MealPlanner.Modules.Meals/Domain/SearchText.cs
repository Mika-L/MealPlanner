using System.Globalization;
using System.Text;

namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Normalisation de texte pour des comparaisons tolérantes (recherche, correspondance
/// d'ingrédients) : minuscule + suppression des accents ("gruyere" ~ "Gruyère râpé").</summary>
public static class SearchText
{
    public static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
