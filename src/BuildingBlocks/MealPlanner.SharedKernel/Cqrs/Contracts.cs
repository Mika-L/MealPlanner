namespace MealPlanner.SharedKernel.Cqrs;

/// <summary>Marqueur d'une commande (écriture) renvoyant <typeparamref name="TResponse"/>.</summary>
public interface ICommand<TResponse>;

/// <summary>Marqueur d'une requête (lecture) renvoyant <typeparamref name="TResponse"/>.</summary>
public interface IQuery<TResponse>;

/// <summary>Handler d'une commande.</summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Handler d'une requête.</summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

/// <summary>Point d'entrée unique pour envoyer commandes et requêtes vers leur handler.</summary>
public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
