namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Saisons pour lesquelles un repas est adapté (combinables).</summary>
[Flags]
public enum Season
{
    None = 0,
    Spring = 1 << 0,
    Summer = 1 << 1,
    Autumn = 1 << 2,
    Winter = 1 << 3,
    AllYear = Spring | Summer | Autumn | Winter,
}
