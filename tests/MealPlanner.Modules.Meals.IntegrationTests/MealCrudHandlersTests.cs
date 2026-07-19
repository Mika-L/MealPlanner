using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.CreateMeal;
using MealPlanner.Modules.Meals.Features.DeleteMeal;
using MealPlanner.Modules.Meals.Features.ListMeals;
using MealPlanner.Modules.Meals.Features.UpdateMeal;
using MealPlanner.Modules.Meals.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.IntegrationTests;

[Collection(nameof(MySqlCollection))]
public sealed class MealCrudHandlersTests(MySqlFixture fixture)
{
    [Fact]
    public async Task Should_create_a_meal_with_its_ingredients()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var command = new CreateMealCommand(
            "Ratatouille",
            "Mijoté de légumes du soleil.",
            [Season.Summer, Season.Autumn],
            [MealStyle.Healthy, MealStyle.Comforting],
            60,
            ["courgette", "aubergine"]);

        var result = await new CreateMealHandler(dbContext).HandleAsync(command, cancellationToken);

        result.IsSuccess.Should().BeTrue();

        var stored = await dbContext.Meals
            .Include(meal => meal.Ingredients)
            .SingleAsync(meal => meal.Id == result.Value, cancellationToken);
        stored.Name.Should().Be("Ratatouille");
        stored.Seasons.Should().Be(Season.Summer | Season.Autumn);
        stored.Styles.Should().Be(MealStyle.Healthy | MealStyle.Comforting);
        stored.Ingredients.Select(ingredient => ingredient.Name)
            .Should().BeEquivalentTo("courgette", "aubergine");
    }

    [Fact]
    public async Task Should_update_a_meal_and_replace_its_ingredients()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var meal = new Meal("Ancien nom", "Ancienne description", Season.Winter, MealStyle.Comforting, 90);
        meal.AddIngredient("bœuf");
        dbContext.Meals.Add(meal);
        await dbContext.SaveChangesAsync(cancellationToken);

        var command = new UpdateMealCommand(
            meal.Id,
            "Nouveau nom",
            "Nouvelle description",
            [Season.Spring],
            [MealStyle.Light],
            20,
            ["salade", "tomate"]);

        var result = await new UpdateMealHandler(dbContext).HandleAsync(command, cancellationToken);

        result.IsSuccess.Should().BeTrue();

        var stored = await dbContext.Meals
            .Include(persisted => persisted.Ingredients)
            .SingleAsync(persisted => persisted.Id == meal.Id, cancellationToken);
        stored.Name.Should().Be("Nouveau nom");
        stored.Seasons.Should().Be(Season.Spring);
        stored.PrepTimeMinutes.Should().Be(20);
        stored.Ingredients.Select(ingredient => ingredient.Name)
            .Should().BeEquivalentTo("salade", "tomate");
    }

    [Fact]
    public async Task Should_return_not_found_when_updating_an_unknown_meal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var command = new UpdateMealCommand(
            Guid.CreateVersion7(), "Nom", "Description", [Season.Summer], [MealStyle.Quick], 15, []);

        var result = await new UpdateMealHandler(dbContext).HandleAsync(command, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(SharedKernel.Results.ErrorType.NotFound);
    }

    [Fact]
    public async Task Should_delete_a_meal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var meal = new Meal("À supprimer", "", Season.AllYear, MealStyle.Quick, 10);
        dbContext.Meals.Add(meal);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await new DeleteMealHandler(dbContext).HandleAsync(
            new DeleteMealCommand(meal.Id), cancellationToken);

        result.IsSuccess.Should().BeTrue();
        (await dbContext.Meals.AnyAsync(persisted => persisted.Id == meal.Id, cancellationToken))
            .Should().BeFalse();
    }

    [Fact]
    public async Task Should_return_not_found_when_deleting_an_unknown_meal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        var result = await new DeleteMealHandler(dbContext).HandleAsync(
            new DeleteMealCommand(Guid.CreateVersion7()), cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(SharedKernel.Results.ErrorType.NotFound);
    }

    [Fact]
    public async Task Should_list_meals_ordered_by_name_with_atomic_seasons()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = await CreateFreshDbContextAsync(cancellationToken);

        dbContext.Meals.AddRange(
            new Meal("Zurek", "Soupe polonaise", Season.Winter, MealStyle.Comforting, 90),
            new Meal("Avocado toast", "Rapide", Season.AllYear, MealStyle.Quick, 5));
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await new ListMealsHandler(dbContext).HandleAsync(new ListMealsQuery(), cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Meals.Select(meal => meal.Name).Should().Equal("Avocado toast", "Zurek");
        // Season.AllYear est éclatée en ses quatre saisons atomiques.
        result.Value.Meals[0].Seasons
            .Should().BeEquivalentTo([Season.Spring, Season.Summer, Season.Autumn, Season.Winter]);
    }

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
