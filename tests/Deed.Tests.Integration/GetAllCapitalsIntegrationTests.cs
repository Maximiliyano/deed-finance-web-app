using Deed.Application.Capitals.Responses;
using Deed.Tests.Common.Mocks;
using FluentAssertions;

namespace Deed.Tests.Integration;

public sealed class GetAllCapitalsIntegrationTests : IDisposable
{
    private readonly TestApiClient _testApiClient;

    private static readonly Uri Endpoint = new("api/capitals/all", UriKind.Relative);

    public GetAllCapitalsIntegrationTests()
    {
        _testApiClient = new TestApiClient();
    }

    public void Dispose()
    {
        _testApiClient.Dispose();
    }

    [Fact]
    public async Task GetAllCapitals_ShouldReturnCollection()
    {
        // Arrange

        // Act
        var capitals = await _testApiClient.SendRequestAsync<IEnumerable<CapitalResponse>>(HttpMethod.Get, Endpoint);

        // Assert
        capitals.Should().NotBeNull();
    }
}
