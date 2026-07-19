using MealPlanner.Modules.Meals.Domain;

namespace MealPlanner.Modules.Meals.UnitTests.Domain;

public sealed class FlagEnumTests
{
    [Fact]
    public void Should_combine_values_into_a_single_flag()
    {
        var combined = FlagEnum.Combine([Season.Spring, Season.Summer]);

        combined.Should().Be(Season.Spring | Season.Summer);
    }

    [Fact]
    public void Should_combine_an_empty_list_into_none()
    {
        var combined = FlagEnum.Combine<MealStyle>([]);

        combined.Should().Be(MealStyle.None);
    }

    [Fact]
    public void Should_decompose_a_combined_flag_into_atomic_values()
    {
        var atomic = FlagEnum.Decompose(MealStyle.Healthy | MealStyle.Quick);

        atomic.Should().Equal(MealStyle.Healthy, MealStyle.Quick);
    }

    [Fact]
    public void Should_decompose_a_composite_flag_into_its_atomic_seasons_only()
    {
        // AllYear = Spring | Summer | Autumn | Winter : on n'expose jamais la valeur composite.
        var atomic = FlagEnum.Decompose(Season.AllYear);

        atomic.Should().Equal(Season.Spring, Season.Summer, Season.Autumn, Season.Winter);
        atomic.Should().NotContain(Season.AllYear);
    }

    [Fact]
    public void Should_decompose_none_into_an_empty_list()
    {
        var atomic = FlagEnum.Decompose(Season.None);

        atomic.Should().BeEmpty();
    }
}
