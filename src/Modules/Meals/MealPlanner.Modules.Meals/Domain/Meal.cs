namespace MealPlanner.Modules.Meals.Domain;

/// <summary>Idée de repas du catalogue, filtrable par saison, style, temps et ingrédients.</summary>
public sealed class Meal
{
    private readonly List<MealIngredient> _ingredients = [];

    // EF Core
    private Meal()
    {
    }

    public Meal(string name, string description, Season seasons, MealStyle styles, int prepTimeMinutes)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Description = description;
        Seasons = seasons;
        Styles = styles;
        PrepTimeMinutes = prepTimeMinutes;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Season Seasons { get; private set; }

    public MealStyle Styles { get; private set; }

    public int PrepTimeMinutes { get; private set; }

    public IReadOnlyCollection<MealIngredient> Ingredients => _ingredients.AsReadOnly();

    public void AddIngredient(string name)
    {
        _ingredients.Add(new MealIngredient(Id, name));
    }
}
