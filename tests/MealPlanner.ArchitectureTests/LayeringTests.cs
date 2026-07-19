using System.Reflection;

using MealPlanner.Modules.Meals;
using MealPlanner.SharedKernel.Cqrs;

using NetArchTest.Rules;

namespace MealPlanner.ArchitectureTests;

public sealed class LayeringTests
{
    private static readonly Assembly SharedKernel = typeof(IDispatcher).Assembly;
    private static readonly Assembly MealsModule = typeof(MealsModule).Assembly;

    private const string ApiNamespace = "MealPlanner.Api";
    private const string ModulesNamespace = "MealPlanner.Modules";

    [Fact]
    public void SharedKernel_should_not_depend_on_any_module_or_the_api()
    {
        var result = Types.InAssembly(SharedKernel)
            .ShouldNot()
            .HaveDependencyOnAny(ModulesNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "le SharedKernel est la couche la plus basse : {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Meals_module_should_not_depend_on_the_api_host()
    {
        var result = Types.InAssembly(MealsModule)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "un module ne connaît pas son hôte : {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_entities_should_not_depend_on_entity_framework()
    {
        var result = Types.InAssembly(MealsModule)
            .That()
            .ResideInNamespace("MealPlanner.Modules.Meals.Domain")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "le domaine reste persistence-ignorant : {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }
}
