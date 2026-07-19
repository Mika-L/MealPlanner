using MealPlanner.Modules.Meals.Features.ListMeals;

namespace MealPlanner.Modules.Meals.UnitTests.Features.ListMeals;

public sealed class ListMealsValidatorTests
{
    private readonly ListMealsValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_query()
    {
        var query = new ListMealsQuery("tomate", Page: 1, PageSize: 24);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_reject_a_page_below_one(int page)
    {
        var query = new ListMealsQuery(null, page, PageSize: 24);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(ListMealsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ListMealsValidator.MaxPageSize + 1)]
    public void Should_reject_a_page_size_outside_bounds(int pageSize)
    {
        var query = new ListMealsQuery(null, Page: 1, pageSize);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(ListMealsQuery.PageSize));
    }
}
