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
        var query = new GenerateMealIdeasQuery(Season.Summer, null, MaxPrepTimeMinutes: 30, null, Count: 10);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().ContainSingle()
            .Which.Name.Should().Be("Salade express");
    }

    [Fact]
    public async Task Should_filter_by_ingredient()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var tomatoSoup = new Meal("Soupe de tomate", "Veloutée", Season.Autumn, MealStyle.Healthy, 20);
        tomatoSoup.AddIngredient("tomate");
        tomatoSoup.AddIngredient("oignon");
        var omelette = new Meal("Omelette", "Rapide", Season.AllYear, MealStyle.Quick, 10);
        omelette.AddIngredient("œuf");
        dbContext.Meals.AddRange(tomatoSoup, omelette);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new GenerateMealIdeasHandler(dbContext);
        var query = new GenerateMealIdeasQuery(null, null, null, IncludeIngredients: ["tomate"], Count: 10);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Ideas.Should().ContainSingle()
            .Which.Name.Should().Be("Soupe de tomate");
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
