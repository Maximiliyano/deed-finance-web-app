using System.Net;
using System.Net.Http.Json;
using Deed.Application.BudgetEstimations.Responses;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.BudgetEstimations;

public sealed class BudgetEstimationsEndpointTests(DeedApiFactory factory)
    : IntegrationTest(factory, "api/budget-estimations")
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
    
    public static IEnumerable<object[]> EstimationsTestData =>
    [
        // Day
        [
            new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc)
        ],

        // Month
        [
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc)
        ],

        // Year
        [
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        ],
    ];
    
    [Theory]
    [MemberData(nameof(EstimationsTestData))]
    public async Task GetAllEstimations_ReturnsResults(DateTime periodStart, DateTime periodEnd)
    {
        int id = await CreateEstimationAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/budget-estimations/all", new
        {
            periodStart,
            periodEnd,
        });
        var estimations = await response.Content.ReadFromJsonAsync<IEnumerable<BudgetEstimationResponse>>();
        Assert.NotNull(estimations);
        Assert.Contains(estimations, e => e.Id.Equals(id));
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
