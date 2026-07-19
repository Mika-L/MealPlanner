using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.SharedKernel.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
    ];

    /// <summary>Enregistre le <see cref="IDispatcher"/> une seule fois (idempotent).</summary>
    public static IServiceCollection AddDispatcher(this IServiceCollection services)
    {
        services.TryAddScoped();
        return services;
    }

    /// <summary>
    /// Scanne l'assembly donné et enregistre en <c>Scoped</c> tous les
    /// <see cref="ICommandHandler{TCommand,TResponse}"/> et <see cref="IQueryHandler{TQuery,TResponse}"/>.
    /// </summary>
    public static IServiceCollection AddCqrsHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var registrations =
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false }
            from @interface in type.GetInterfaces()
            where @interface.IsGenericType && HandlerInterfaces.Contains(@interface.GetGenericTypeDefinition())
            select (Service: @interface, Implementation: type);

        foreach (var (service, implementation) in registrations)
        {
            services.AddScoped(service, implementation);
        }

        return services;
    }

    private static void TryAddScoped(this IServiceCollection services)
    {
        if (services.All(descriptor => descriptor.ServiceType != typeof(IDispatcher)))
        {
            services.AddScoped<IDispatcher, Dispatcher>();
        }
    }
}
