using System.Net;

namespace MealPlanner.Api.FunctionalTests;

public sealed class AuthorizationTests(MealPlannerApiFactory factory) : IClassFixture<MealPlannerApiFactory>
{
    [Fact]
    public async Task Meals_endpoint_should_reject_unauthenticated_requests()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        // Pas de header Authorization : l'utilisateur doit être authentifié pour voir ses recettes.
        var response = await client.GetAsync("/api/meals", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
