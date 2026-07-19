using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.SharedKernel.Cqrs;

/// <summary>
/// Résout le handler correspondant au type concret de la commande/requête et lui délègue
/// l'exécution via la méthode de l'interface (publique), ce qui autorise des handlers <c>internal</c>.
/// Enregistré en <c>Scoped</c>.
/// </summary>
internal sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    private const string HandleMethodName = "HandleAsync";

    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        return Invoke<TResponse>(handlerType, command, cancellationToken);
    }

    public Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        return Invoke<TResponse>(handlerType, query, cancellationToken);
    }

    private Task<TResponse> Invoke<TResponse>(Type handlerType, object message, CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(HandleMethodName)
            ?? throw new InvalidOperationException($"Méthode {HandleMethodName} introuvable sur {handlerType}.");

        return (Task<TResponse>)method.Invoke(handler, [message, cancellationToken])!;
    }
}
