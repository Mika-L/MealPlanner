namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Conversions entre un flags enum combiné et la liste de ses valeurs atomiques (un seul bit).</summary>
public static class FlagEnum
{
    /// <summary>Combine une liste de valeurs (ex. <c>[Spring, Summer]</c>) en un flag unique via OU binaire.</summary>
    public static TEnum Combine<TEnum>(IEnumerable<TEnum> flags)
        where TEnum : struct, Enum
    {
        long combined = 0;
        foreach (var flag in flags)
        {
            combined |= Convert.ToInt64(flag);
        }

        return (TEnum)Enum.ToObject(typeof(TEnum), combined);
    }

    /// <summary>
    /// Éclate un flag combiné en ses valeurs atomiques présentes (un seul bit), en excluant
    /// <c>None</c> et les valeurs composites (ex. <c>Season.AllYear</c> → <c>[Spring, Summer, Autumn, Winter]</c>).
    /// </summary>
    public static IReadOnlyList<TEnum> Decompose<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Where(flag => IsSingleBit(Convert.ToInt64(flag)) && value.HasFlag(flag))
            .ToList();

    private static bool IsSingleBit(long value) => value != 0 && (value & (value - 1)) == 0;
}
