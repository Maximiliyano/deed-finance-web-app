using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Capitals;

public sealed class CapitalsEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
{
    private async Task<int> CreateCapitalAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/capitals", new
        {
            name = $"Cap_{Guid.NewGuid().ToString("N")[..8]}",
            balance = 5000m,
            currency = 1,
            includeInTotal = true,
            onlyForSavings = false
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateCapital_ReturnsId()
    {
        int id = await CreateCapitalAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllCapitals_ReturnsResults()
    {
        await CreateCapitalAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/capitals/all", new
        {
            searchTerm = (string?)null,
            sortBy = (string?)null,
            sortDirection = (string?)null,
            filterBy = (string?)null
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCapital_ReturnsSuccess()
    {
        int id = await CreateCapitalAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/capitals/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PatchIncludeInTotal_ReturnsSuccess()
    {
        int id = await CreateCapitalAsync();

        HttpRequestMessage request = new(HttpMethod.Patch, $"/api/capitals/{id}/include-in-total")
        {
            Content = JsonContent.Create(false)
        };
        HttpResponseMessage response = await Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
