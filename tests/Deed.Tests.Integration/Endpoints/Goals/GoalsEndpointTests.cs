using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Goals;

public sealed class GoalsEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
{
    private async Task<int> CreateGoalAsync(string? title = null)
    {
        title ??= $"Goal_{Guid.NewGuid().ToString("N")[..8]}";

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/goals", new
        {
            title,
            targetAmount = 10000m,
            currency = 1,
            currentAmount = 0m,
            deadline = (string?)null,
            note = (string?)null
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateGoal_ReturnsId()
    {
        int id = await CreateGoalAsync();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllGoals_ReturnsResults()
    {
        await CreateGoalAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/goals");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateGoal_ReturnsSuccess()
    {
        string title = $"Goal_{Guid.NewGuid().ToString("N")[..8]}";
        int id = await CreateGoalAsync(title);

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/goals/{id}", new
        {
            title,
            targetAmount = 10000m,
            currency = 1,
            currentAmount = 500m,
            deadline = (string?)null,
            note = (string?)null,
            isCompleted = false
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteGoal_ReturnsSuccess()
    {
        int id = await CreateGoalAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/goals/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
