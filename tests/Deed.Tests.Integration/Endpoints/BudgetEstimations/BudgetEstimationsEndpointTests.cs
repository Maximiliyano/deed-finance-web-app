using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.BudgetEstimations;

public sealed class BudgetEstimationsEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
{
    private async Task<int> CreateEstimationAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/budget-estimations", new
        {
            description = $"Est_{Guid.NewGuid().ToString("N")[..8]}",
            budgetAmount = 500m,
            budgetCurrency = 1,
            capitalId = (int?)null
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateEstimation_ReturnsId()
    {
        int id = await CreateEstimationAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllEstimations_ReturnsResults()
    {
        await CreateEstimationAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/budget-estimations");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteEstimation_ReturnsSuccess()
    {
        int id = await CreateEstimationAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/budget-estimations/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
