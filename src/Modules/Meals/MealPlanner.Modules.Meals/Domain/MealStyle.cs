namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Registres/ambiances d'un repas (combinables) : healthy, réconfortant, etc.</summary>
[Flags]
public enum MealStyle
{
    None = 0,
    Healthy = 1 << 0,
    Comforting = 1 << 1,
    Quick = 1 << 2,
    Festive = 1 << 3,
    Light = 1 << 4,
    Gourmet = 1 << 5,
}
