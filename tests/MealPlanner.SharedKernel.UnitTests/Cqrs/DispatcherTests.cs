using MealPlanner.SharedKernel.Cqrs;

using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.SharedKernel.UnitTests.Cqrs;

public sealed class DispatcherTests
{
    [Fact]
    public async Task QueryAsync_should_route_to_the_matching_handler()
    {
        var dispatcher = BuildDispatcher();

        var result = await dispatcher.QueryAsync(new PingQuery("bonjour"), TestContext.Current.CancellationToken);

        result.Should().Be("pong:bonjour");
    }

    [Fact]
    public async Task SendAsync_should_route_to_the_matching_handler()
    {
        var dispatcher = BuildDispatcher();

        var result = await dispatcher.SendAsync(new AddCommand(2, 3), TestContext.Current.CancellationToken);

        result.Should().Be(5);
    }

    private static IDispatcher BuildDispatcher()
    {
        var provider = new ServiceCollection()
            .AddDispatcher()
            .AddCqrsHandlersFromAssembly(typeof(DispatcherTests).Assembly)
            .BuildServiceProvider();

        return provider.GetRequiredService<IDispatcher>();
    }

    // Handlers internal au projet de test : reproduit le cas d'un handler non visible depuis le SharedKernel.
    internal sealed record PingQuery(string Message) : IQuery<string>;

    internal sealed class PingQueryHandler : IQueryHandler<PingQuery, string>
    {
        public Task<string> HandleAsync(PingQuery query, CancellationToken cancellationToken)
            => Task.FromResult($"pong:{query.Message}");
    }

    internal sealed record AddCommand(int Left, int Right) : ICommand<int>;

    internal sealed class AddCommandHandler : ICommandHandler<AddCommand, int>
    {
        public Task<int> HandleAsync(AddCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.Left + command.Right);
    }
}
