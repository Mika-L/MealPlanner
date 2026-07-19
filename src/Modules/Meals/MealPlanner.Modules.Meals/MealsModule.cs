using FluentValidation;

using MealPlanner.Modules.Meals.Features.CreateMeal;
using MealPlanner.Modules.Meals.Features.DeleteMeal;
using MealPlanner.Modules.Meals.Features.GenerateMealIdeas;
using MealPlanner.Modules.Meals.Features.ListMeals;
using MealPlanner.Modules.Meals.Features.UpdateMeal;
using MealPlanner.Modules.Meals.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;
using MealPlanner.SharedKernel.Integration;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Modules.Meals;

/// <summary>Composition du module Meals : persistance, handlers CQRS, validateurs, endpoints.</summary>
public static class MealsModule
{
    public const string ConnectionStringName = "MealsDb";

    public static IServiceCollection AddMealsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Chaîne de connexion '{ConnectionStringName}' introuvable.");

        services.AddDbContext<MealsDbContext>(options => options.UseMySQL(connectionString));

        services.AddDispatcher();
        services.AddCqrsHandlersFromAssembly(typeof(MealsModule).Assembly);
        services.AddValidatorsFromAssemblyContaining<GenerateMealIdeasValidator>(includeInternalTypes: true);

        // À l'inscription d'un utilisateur, on lui clone le catalogue de démarrage.
        services.AddScoped<IUserRegisteredListener, UserCatalogSeeder>();

        return services;
    }

    public static IEndpointRouteBuilder MapMealsModule(this IEndpointRouteBuilder endpoints)
    {
        GenerateMealIdeasEndpoint.Map(endpoints);
        ListMealsEndpoint.Map(endpoints);
        CreateMealEndpoint.Map(endpoints);
        UpdateMealEndpoint.Map(endpoints);
        DeleteMealEndpoint.Map(endpoints);
        return endpoints;
    }

    /// <summary>Applique les migrations du module. À appeler au démarrage. Le catalogue est cloné par
    /// utilisateur à l'inscription (voir <see cref="UserCatalogSeeder"/>), plus de seed global.</summary>
    public static async Task InitializeMealsModuleAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MealsDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
