namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Ingrédient rattaché à un <see cref="Meal"/>.</summary>
public sealed class MealIngredient
{
    // EF Core
    private MealIngredient()
    {
    }

    public MealIngredient(Guid mealId, string name)
    {
        Id = Guid.CreateVersion7();
        MealId = mealId;
        Name = name;
    }

    public Guid Id { get; private set; }

    public Guid MealId { get; private set; }

    public string Name { get; private set; } = string.Empty;
}
