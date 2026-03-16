using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Transfers;

public sealed class TransfersEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
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

    private async Task<int> CreateTransferAsync()
    {
        int sourceId = await CreateCapitalAsync();
        int destId = await CreateCapitalAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/transfers", new
        {
            sourceCapitalId = sourceId,
            destinationCapitalId = destId,
            amount = 100m,
            destinationAmount = 100m
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateTransfer_ReturnsId()
    {
        int id = await CreateTransferAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllTransfers_ReturnsResults()
    {
        await CreateTransferAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/transfers");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTransfer_ReturnsSuccess()
    {
        int id = await CreateTransferAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/transfers/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
