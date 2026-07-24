using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.ReplaceMealIdea;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Identity;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.IntegrationTests;

[Collection(nameof(MsSqlCollection))]
public sealed class ReplaceMealIdeaHandlerTests(MsSqlFixture fixture)
{
    private static readonly Guid OwnerId = Guid.CreateVersion7();
    private readonly ICurrentUser _currentUser = new StubCurrentUser(OwnerId);

    [Fact]
    public async Task Should_return_an_alternative_matching_the_filters()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Salade express", "", Season.Summer, MealStyle.Healthy, 15);
        var greekSalad = new Meal(OwnerId, "Salade grecque", "", Season.Summer, MealStyle.Healthy, 20);
        var winterStew = new Meal(OwnerId, "Pot-au-feu", "", Season.Winter, MealStyle.Comforting, 120);
        dbContext.Meals.AddRange(replaced, greekSalad, winterStew);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            Season.Summer, null, null, null, Day: 3, replaced.Id, KeptMealIds: [], SeenMealIds: []);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        // Seule la recette d'été autre que celle remplacée convient ; le plat d'hiver est écarté.
        result.Value.Meal.Name.Should().Be("Salade grecque");
        result.Value.Meal.Day.Should().Be(3);
    }

    [Fact]
    public async Task Should_not_repick_the_replaced_or_kept_meals()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Repas remplacé", "", Season.AllYear, MealStyle.Quick, 10);
        var kept = new Meal(OwnerId, "Repas conservé", "", Season.AllYear, MealStyle.Quick, 12);
        var free = new Meal(OwnerId, "Repas libre", "", Season.AllYear, MealStyle.Quick, 15);
        dbContext.Meals.AddRange(replaced, kept, free);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            null, null, null, null, Day: 1, replaced.Id, KeptMealIds: [kept.Id], SeenMealIds: []);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Meal.Id.Should().Be(free.Id);
    }

    [Fact]
    public async Task Should_prefer_a_meal_using_an_available_ingredient()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Repas remplacé", "", Season.AllYear, MealStyle.Quick, 5);
        // L'omelette est plus rapide, mais la soupe utilise un ingrédient dispo : elle passe devant.
        var omelette = new Meal(OwnerId, "Omelette", "", Season.AllYear, MealStyle.Quick, 10);
        omelette.AddIngredient("œuf");
        var tomatoSoup = new Meal(OwnerId, "Soupe de tomate", "", Season.AllYear, MealStyle.Healthy, 20);
        tomatoSoup.AddIngredient("tomate");
        dbContext.Meals.AddRange(replaced, omelette, tomatoSoup);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            null, null, null, IncludeIngredients: ["tomate"], Day: 1, replaced.Id, KeptMealIds: [], SeenMealIds: []);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Meal.Name.Should().Be("Soupe de tomate");
        result.Value.Meal.MatchedIngredients.Should().Contain("tomate");
    }

    [Fact]
    public async Task Should_avoid_ingredients_already_consumed_by_kept_meals()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Repas remplacé", "", Season.AllYear, MealStyle.Quick, 5);
        // Le repas conservé consomme déjà le jambon.
        var keptSalad = new Meal(OwnerId, "Salade au jambon", "", Season.AllYear, MealStyle.Light, 15);
        keptSalad.AddIngredient("jambon");
        // Candidate reposant uniquement sur le jambon : elle doit être écartée (jambon déjà pris).
        var hamOmelette = new Meal(OwnerId, "Omelette au jambon", "", Season.AllYear, MealStyle.Quick, 10);
        hamOmelette.AddIngredient("jambon");
        // Candidate neutre : elle complète le planning.
        var plainRice = new Meal(OwnerId, "Riz nature", "", Season.AllYear, MealStyle.Quick, 20);
        dbContext.Meals.AddRange(replaced, keptSalad, hamOmelette, plainRice);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            null, null, null, IncludeIngredients: ["jambon"], Day: 1, replaced.Id, KeptMealIds: [keptSalad.Id],
            SeenMealIds: []);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Meal.Name.Should().Be("Riz nature");
        result.Value.Meal.MatchedIngredients.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_return_a_different_alternative_each_time_until_the_pool_is_exhausted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Repas de départ", "", Season.AllYear, MealStyle.Quick, 5);
        var first = new Meal(OwnerId, "Alternative rapide", "", Season.AllYear, MealStyle.Quick, 10);
        var second = new Meal(OwnerId, "Alternative moyenne", "", Season.AllYear, MealStyle.Quick, 20);
        var third = new Meal(OwnerId, "Alternative lente", "", Season.AllYear, MealStyle.Quick, 30);
        dbContext.Meals.AddRange(replaced, first, second, third);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);

        // On rejoue « Remplacer » en accumulant l'historique des recettes déjà proposées pour ce jour :
        // chaque appel doit renvoyer une recette encore jamais vue, sans jamais ping-ponger.
        var seen = new List<Guid> { replaced.Id };
        var proposed = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            var query = new ReplaceMealIdeaQuery(
                null, null, null, null, Day: 1, replaced.Id, KeptMealIds: [], SeenMealIds: seen);

            var result = await handler.HandleAsync(query, cancellationToken);

            result.IsSuccess.Should().BeTrue();
            proposed.Add(result.Value.Meal.Id);
            seen.Add(result.Value.Meal.Id);
        }

        proposed.Should().Equal(first.Id, second.Id, third.Id);
    }

    [Fact]
    public async Task Should_restart_the_cycle_without_repeating_the_current_meal_once_exhausted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var first = new Meal(OwnerId, "Alternative rapide", "", Season.AllYear, MealStyle.Quick, 10);
        var second = new Meal(OwnerId, "Alternative moyenne", "", Season.AllYear, MealStyle.Quick, 20);
        // La recette actuellement affichée est la dernière du cycle ; toutes les autres ont déjà été vues.
        var current = new Meal(OwnerId, "Alternative lente", "", Season.AllYear, MealStyle.Quick, 30);
        dbContext.Meals.AddRange(first, second, current);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            null, null, null, null, Day: 1, current.Id,
            KeptMealIds: [], SeenMealIds: [first.Id, second.Id, current.Id]);

        var result = await handler.HandleAsync(query, cancellationToken);

        // Pool épuisé : le cycle repart sur la plus rapide, jamais sur la recette affichée.
        result.IsSuccess.Should().BeTrue();
        result.Value.Meal.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task Should_fail_when_no_other_recipe_matches()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var replaced = new Meal(OwnerId, "Repas remplacé", "", Season.AllYear, MealStyle.Quick, 10);
        var kept = new Meal(OwnerId, "Repas conservé", "", Season.AllYear, MealStyle.Quick, 12);
        dbContext.Meals.AddRange(replaced, kept);
        await dbContext.SaveChangesAsync(cancellationToken);

        var handler = new ReplaceMealIdeaHandler(dbContext, _currentUser);
        var query = new ReplaceMealIdeaQuery(
            null, null, null, null, Day: 1, replaced.Id, KeptMealIds: [kept.Id], SeenMealIds: []);

        var result = await handler.HandleAsync(query, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    // Schéma remis à zéro à chaque test : les tests de la collection partagent le même conteneur.
    private async Task<MealsDbContext> CreateFreshDbContextAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<MealsDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;

        var dbContext = new MealsDbContext(options);
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        return dbContext;
    }
}
