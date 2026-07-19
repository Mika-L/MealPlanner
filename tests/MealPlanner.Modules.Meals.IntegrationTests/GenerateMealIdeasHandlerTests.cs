using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;
using MealPlanner.Modules.Meals.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.IntegrationTests;

[Collection(nameof(MySqlCollection))]
public sealed class GenerateMealIdeasHandlerTests(MySqlFixture fixture)
{
    [Fact]
    public async Task Should_return_meals_matching_season_and_prep_time()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var winterStew = new Meal("Pot-au-feu", "Plat mijoté d'hiver", Season.Winter, MealStyle.Comforting, 120);
        var quickSalad = new Meal("Salade express", "Salade estivale rapide", Season.Summer, MealStyle.Healthy | MealStyle.Quick, 15);
        dbContext.Meals.AddRange(winterStew, quickSalad);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(Season.Summer, null, MaxPrepTimeMinutes: 30, null, Days: 7);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().ContainSingle()
            .Which.Name.Should().Be("Salade express");
    }

    [Fact]
    public async Task Should_number_the_plan_by_day()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        dbContext.Meals.AddRange(
            new Meal("Repas A", "", Season.AllYear, MealStyle.Quick, 10),
            new Meal("Repas B", "", Season.AllYear, MealStyle.Quick, 20),
            new Meal("Repas C", "", Season.AllYear, MealStyle.Quick, 30));
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, null, Days: 2);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().HaveCount(2);
        result.Value.Ideas.Select(idea => (idea.Day, idea.Name))
            .Should().Equal((1, "Repas A"), (2, "Repas B"));
    }

    [Fact]
    public async Task Should_prioritise_meals_using_available_ingredients()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        // L'omelette est plus rapide : sans priorité elle passerait devant.
        var omelette = new Meal("Omelette", "Rapide", Season.AllYear, MealStyle.Quick, 10);
        omelette.AddIngredient("œuf");
        var tomatoSoup = new Meal("Soupe de tomate", "Veloutée", Season.Autumn, MealStyle.Healthy, 20);
        tomatoSoup.AddIngredient("tomate");
        tomatoSoup.AddIngredient("oignon");
        dbContext.Meals.AddRange(omelette, tomatoSoup);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, IncludeIngredients: ["tomate"], Days: 7);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().HaveCount(2);
        // Le repas utilisant la tomate est placé en jour 1, les autres complètent le planning.
        var firstDay = result.Value.Ideas[0];
        firstDay.Name.Should().Be("Soupe de tomate");
        firstDay.MatchedIngredients.Should().Contain("tomate");
        result.Value.Ideas[1].Name.Should().Be("Omelette");
        result.Value.Ideas[1].MatchedIngredients.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_match_ingredient_ignoring_case_and_accents()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var gratin = new Meal("Gratin", "Fondant", Season.Winter, MealStyle.Comforting, 45);
        gratin.AddIngredient("Gruyère râpé");
        dbContext.Meals.Add(gratin);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, IncludeIngredients: ["gruyere"], Days: 7);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().ContainSingle()
            .Which.Name.Should().Be("Gratin");
    }

    [Fact]
    public async Task Should_not_reuse_the_same_ingredient_across_two_meals()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        // Deux recettes ne reposant que sur le jambon : une seule tranche ne fait pas les deux plats,
        // et la seconde ne doit pas non plus revenir comme repas de complément.
        var omelette = new Meal("Omelette au jambon", "Rapide", Season.AllYear, MealStyle.Quick, 10);
        omelette.AddIngredient("œuf");
        omelette.AddIngredient("jambon");
        var salad = new Meal("Salade au jambon", "Fraîche", Season.AllYear, MealStyle.Light, 15);
        salad.AddIngredient("salade");
        salad.AddIngredient("jambon");
        dbContext.Meals.AddRange(omelette, salad);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, IncludeIngredients: ["jambon"], Days: 7);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        // La plus rapide (omelette, 10 min) consomme le jambon ; la salade est écartée.
        result.Value.Ideas.Should().ContainSingle()
            .Which.Name.Should().Be("Omelette au jambon");
    }

    [Fact]
    public async Task Should_keep_meals_using_distinct_available_ingredients()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var salad = new Meal("Salade au jambon", "Fraîche", Season.AllYear, MealStyle.Light, 15);
        salad.AddIngredient("jambon");
        salad.AddIngredient("tomate");
        var gratin = new Meal("Gratin au gruyère", "Fondant", Season.AllYear, MealStyle.Comforting, 45);
        gratin.AddIngredient("gruyère");
        gratin.AddIngredient("pomme de terre");
        dbContext.Meals.AddRange(salad, gratin);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, IncludeIngredients: ["jambon", "gruyère"], Days: 7);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        // Chaque plat s'appuie sur un ingrédient distinct : les deux sont cuisinables.
        result.Value.Ideas.Select(idea => idea.Name)
            .Should().BeEquivalentTo("Salade au jambon", "Gratin au gruyère");
    }

    [Fact]
    public async Task Should_limit_the_plan_to_the_requested_number_of_days()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        for (var index = 0; index < 5; index++)
        {
            dbContext.Meals.Add(new Meal($"Repas {index}", "", Season.AllYear, MealStyle.Quick, 10 + index));
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, null, Days: 3);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().HaveCount(3);
    }

    // Schéma remis à zéro à chaque test : les tests de la collection partagent le même conteneur.
    private async Task<MealsDbContext> CreateFreshDbContextAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<MealsDbContext>()
            .UseMySQL(fixture.ConnectionString)
            .Options;

        var dbContext = new MealsDbContext(options);
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        return dbContext;
    }
}
