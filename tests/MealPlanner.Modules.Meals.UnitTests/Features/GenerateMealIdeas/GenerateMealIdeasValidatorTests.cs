using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

namespace MealPlanner.Modules.Meals.UnitTests.Features.GenerateMealIdeas;

public sealed class GenerateMealIdeasValidatorTests
{
    private readonly GenerateMealIdeasValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_query()
    {
        var query = new GenerateMealIdeasQuery(Season.Winter, MealStyle.Comforting, 45, null, Days: 7);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Should_reject_a_number_of_days_outside_bounds(int days)
    {
        var query = new GenerateMealIdeasQuery(null, null, null, null, days);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(GenerateMealIdeasQuery.Days));
    }

    [Fact]
    public void Should_reject_a_non_positive_prep_time()
    {
        var query = new GenerateMealIdeasQuery(null, null, MaxPrepTimeMinutes: 0, null, Days: 5);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
