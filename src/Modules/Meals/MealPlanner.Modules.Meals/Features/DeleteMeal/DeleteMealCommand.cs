using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Results;

namespace MealPlanner.Modules.Meals.Features.DeleteMeal;

/// <summary>Supprime une recette du catalogue.</summary>
public sealed record DeleteMealCommand(Guid Id) : ICommand<Result>;
