using MealPlanner.SharedKernel.Results;

namespace MealPlanner.SharedKernel.UnitTests.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_should_expose_value_and_no_error()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_should_expose_error_and_throw_on_value_access()
    {
        var error = Error.NotFound("meal.not_found", "Aucun repas trouvé.");

        var result = Result.Failure<int>(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_conversion_should_wrap_value_as_success()
    {
        Result<string> result = "poulet rôti";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("poulet rôti");
    }
}
