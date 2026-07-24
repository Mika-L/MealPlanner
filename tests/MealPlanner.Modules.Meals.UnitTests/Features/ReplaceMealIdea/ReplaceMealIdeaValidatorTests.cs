using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.ReplaceMealIdea;

namespace MealPlanner.Modules.Meals.UnitTests.Features.ReplaceMealIdea;

public sealed class ReplaceMealIdeaValidatorTests
{
    private readonly ReplaceMealIdeaValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_query()
    {
        var query = new ReplaceMealIdeaQuery(
            Season.Winter, MealStyle.Comforting, 45, null, Day: 1, Guid.CreateVersion7(), [], []);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_reject_a_non_positive_day()
    {
        var query = new ReplaceMealIdeaQuery(
            null, null, null, null, Day: 0, Guid.CreateVersion7(), [], []);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(ReplaceMealIdeaQuery.Day));
    }

    [Fact]
    public void Should_reject_a_non_positive_prep_time()
    {
        var query = new ReplaceMealIdeaQuery(
            null, null, MaxPrepTimeMinutes: 0, null, Day: 1, Guid.CreateVersion7(), [], []);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
