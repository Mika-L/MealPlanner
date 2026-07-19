using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Features.CreateMeal;

namespace MealPlanner.Modules.Meals.UnitTests.Features.CreateMeal;

public sealed class CreateMealValidatorTests
{
    private readonly CreateMealValidator _validator = new();

    private static CreateMealCommand ValidCommand() => new(
        "Ratatouille",
        "Mijoté de légumes.",
        [Season.Summer],
        [MealStyle.Healthy],
        60,
        ["courgette", "aubergine"]);

    [Fact]
    public void Should_accept_a_valid_command()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_reject_an_empty_name()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(failure => failure.PropertyName == nameof(CreateMealCommand.Name));
    }

    [Fact]
    public void Should_reject_a_non_positive_prep_time()
    {
        var command = ValidCommand() with { PrepTimeMinutes = 0 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_reject_a_blank_ingredient()
    {
        var command = ValidCommand() with { Ingredients = ["courgette", "  "] };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
