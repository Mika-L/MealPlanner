using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

namespace MealPlanner.Modules.Meals.UnitTests.Features.GenerateMealIdeas;

public sealed class GenerateMealIdeasValidatorTests
{
    private readonly GenerateMealIdeasValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_query()
    {
        var query = new GenerateMealIdeasQuery(Season.Winter, MealStyle.Comforting, 45, null, Count: 10);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Should_reject_a_count_outside_bounds(int count)
    {
        var query = new GenerateMealIdeasQuery(null, null, null, null, count);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(GenerateMealIdeasQuery.Count));
    }

    [Fact]
    public void Should_reject_a_non_positive_prep_time()
    {
        var query = new GenerateMealIdeasQuery(null, null, MaxPrepTimeMinutes: 0, null, Count: 5);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
