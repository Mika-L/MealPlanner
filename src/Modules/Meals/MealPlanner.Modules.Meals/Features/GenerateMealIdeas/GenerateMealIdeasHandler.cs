using System.Linq.Expressions;

using MealPlanner.Modules.Meals.Domain;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Modules.Meals.Features.GenerateMealIdeas;

internal sealed class GenerateMealIdeasHandler(MealsDbContext dbContext)
    : IQueryHandler<GenerateMealIdeasQuery, Result<GenerateMealIdeasResponse>>
{
    public async Task<Result<GenerateMealIdeasResponse>> HandleAsync(
        GenerateMealIdeasQuery query,
        CancellationToken cancellationToken)
    {
        var meals = dbContext.Meals
            .Include(meal => meal.Ingredients)
            .AsQueryable();

        if (query.Season is { } season && season != Season.None)
        {
            meals = meals.Where(meal => (meal.Seasons & season) != Season.None);
        }

        if (query.Styles is { } styles && styles != MealStyle.None)
        {
            meals = meals.Where(meal => (meal.Styles & styles) != MealStyle.None);
        }

        if (query.MaxPrepTimeMinutes is { } maxPrepTime)
        {
            meals = meals.Where(meal => meal.PrepTimeMinutes <= maxPrepTime);
        }

        if (query.IncludeIngredients is { Count: > 0 } includeIngredients)
        {
            // Sous-requête sur les ingrédients + prédicat OR d'égalités (littéraux inlinés), puis
            // `MealId IN (...)`. Évite un paramètre collection dans Contains, non supporté par le
            // provider MySQL Oracle (InvalidOperationException "does not have a type mapping").
            var matchingMealIds = dbContext.Ingredients
                .Where(MatchesAnyName(includeIngredients))
                .Select(ingredient => ingredient.MealId);

            meals = meals.Where(meal => matchingMealIds.Contains(meal.Id));
        }

        var ideas = await meals
            .OrderBy(meal => meal.PrepTimeMinutes)
            .Take(query.Count)
            .Select(meal => new MealIdea(
                meal.Id,
                meal.Name,
                meal.Description,
                meal.PrepTimeMinutes,
                meal.Ingredients.Select(ingredient => ingredient.Name).ToList()))
            .ToListAsync(cancellationToken);

        return Result.Success(new GenerateMealIdeasResponse(ideas));
    }

    // Construit `ingredient => ingredient.Name == "a" || ingredient.Name == "b" || ...`
    private static Expression<Func<MealIngredient, bool>> MatchesAnyName(IReadOnlyList<string> names)
    {
        var parameter = Expression.Parameter(typeof(MealIngredient), "ingredient");
        var nameProperty = Expression.Property(parameter, nameof(MealIngredient.Name));

        Expression body = Expression.Constant(false);
        foreach (var name in names)
        {
            var equals = Expression.Equal(nameProperty, Expression.Constant(name));
            body = Expression.OrElse(body, equals);
        }

        return Expression.Lambda<Func<MealIngredient, bool>>(body, parameter);
    }
}
