using System.Net;

namespace MealPlanner.Api.FunctionalTests;

[Collection(nameof(MealPlannerApiCollection))]
public sealed class HealthEndpointTests(MealPlannerApiFactory factory)
{
    [Fact]
    public async Task Health_endpoint_should_return_ok()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        body.Should().Contain("ok");
    }
}
