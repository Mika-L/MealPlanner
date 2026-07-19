using MealPlanner.Modules.Meals.Domain;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Infrastructure;

/// <summary>Alimente le catalogue avec un jeu de repas de départ, uniquement s'il est vide.</summary>
public static class MealsDataSeeder
{
    public static async Task SeedAsync(MealsDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Meals.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Meals.AddRange(BuildCatalog());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<Meal> BuildCatalog()
    {
        yield return Create(
            "Salade tomate-mozzarella", "Salade fraîche et rapide pour les beaux jours.",
            Season.Summer, MealStyle.Healthy | MealStyle.Light | MealStyle.Quick, 15,
            "tomate", "mozzarella", "basilic", "huile d'olive");

        yield return Create(
            "Ratatouille", "Mijoté de légumes du soleil, réconfortant et sain.",
            Season.Summer | Season.Autumn, MealStyle.Healthy | MealStyle.Comforting, 60,
            "courgette", "aubergine", "poivron", "tomate", "oignon");

        yield return Create(
            "Pot-au-feu", "Le grand classique mijoté de l'hiver.",
            Season.Winter, MealStyle.Comforting, 150,
            "bœuf", "carotte", "poireau", "navet", "pomme de terre");

        yield return Create(
            "Velouté de potiron", "Soupe onctueuse et légère de saison.",
            Season.Autumn | Season.Winter, MealStyle.Healthy | MealStyle.Comforting | MealStyle.Light, 40,
            "potiron", "oignon", "crème", "bouillon");

        yield return Create(
            "Gratin dauphinois", "Gratin fondant et généreux.",
            Season.Autumn | Season.Winter, MealStyle.Comforting | MealStyle.Gourmet, 90,
            "pomme de terre", "crème", "lait", "ail");

        yield return Create(
            "Buddha bowl au quinoa", "Bol complet, coloré et équilibré.",
            Season.Spring | Season.Summer, MealStyle.Healthy | MealStyle.Light, 25,
            "quinoa", "avocat", "pois chiche", "carotte", "épinard");

        yield return Create(
            "Omelette aux herbes", "Prête en un clin d'œil, toute l'année.",
            Season.AllYear, MealStyle.Healthy | MealStyle.Quick, 10,
            "œuf", "persil", "ciboulette", "beurre");

        yield return Create(
            "Risotto aux champignons", "Crémeux et savoureux, parfait à l'automne.",
            Season.Autumn | Season.Winter, MealStyle.Comforting | MealStyle.Gourmet, 45,
            "riz arborio", "champignon", "parmesan", "oignon", "bouillon");

        yield return Create(
            "Tarte aux fraises", "Dessert festif de printemps.",
            Season.Spring | Season.Summer, MealStyle.Festive | MealStyle.Gourmet, 50,
            "fraise", "pâte sablée", "crème pâtissière", "sucre");

        yield return Create(
            "Curry de légumes", "Plat parfumé, doux et réconfortant.",
            Season.AllYear, MealStyle.Healthy | MealStyle.Comforting, 35,
            "lait de coco", "curry", "pois chiche", "épinard", "tomate");

        yield return Create(
            "Poulet rôti et légumes", "Un plat convivial du dimanche.",
            Season.Autumn | Season.Winter, MealStyle.Comforting | MealStyle.Festive, 80,
            "poulet", "pomme de terre", "carotte", "thym", "ail");

        yield return Create(
            "Wrap poulet-crudités", "Déjeuner léger et express à emporter.",
            Season.Spring | Season.Summer, MealStyle.Quick | MealStyle.Light, 15,
            "tortilla", "poulet", "salade", "tomate", "yaourt");

        yield return Create(
            "Salade au jambon", "Salade complète et fraîche, prête en un instant.",
            Season.AllYear, MealStyle.Light | MealStyle.Healthy | MealStyle.Quick, 15,
            "salade", "jambon", "tomate", "œuf");

        yield return Create(
            "Croque-monsieur", "Le réconfort express jambon-fromage.",
            Season.AllYear, MealStyle.Comforting | MealStyle.Quick, 20,
            "pain de mie", "jambon", "gruyère", "beurre");

        yield return Create(
            "Quiche lorraine", "La tarte salée généreuse du répertoire classique.",
            Season.AllYear, MealStyle.Comforting | MealStyle.Gourmet, 50,
            "pâte brisée", "œuf", "lardons", "crème", "gruyère");
    }

    private static Meal Create(
        string name,
        string description,
        Season seasons,
        MealStyle styles,
        int prepTimeMinutes,
        params string[] ingredients)
    {
        var meal = new Meal(name, description, seasons, styles, prepTimeMinutes);
        foreach (var ingredient in ingredients)
        {
            meal.AddIngredient(ingredient);
        }

        return meal;
    }
}
