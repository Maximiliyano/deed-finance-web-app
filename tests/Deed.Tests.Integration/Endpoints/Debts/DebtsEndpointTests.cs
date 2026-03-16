using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Debts;

public sealed class DebtsEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
{
    private async Task<(int Id, string Item)> CreateDebtAsync()
    {
        string item = $"Debt_{Guid.NewGuid().ToString("N")[..8]}";

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/debts", new
        {
            item,
            amount = 1500m,
            currency = 2,
            source = "John",
            recipient = "Me",
            borrowedAt = "2026-01-01T00:00:00+00:00",
            deadlineAt = (string?)null,
            note = (string?)null,
            capitalId = (int?)null
        });
        response.EnsureSuccessStatusCode();
        int id = await response.Content.ReadFromJsonAsync<int>();
        return (id, item);
    }

    [Fact]
    public async Task CreateDebt_ReturnsId()
    {
        (int id, _) = await CreateDebtAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllDebts_ReturnsResults()
    {
        await CreateDebtAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/debts");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateDebt_MarkAsPaid()
    {
        (int id, string item) = await CreateDebtAsync();

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/debts/{id}", new
        {
            item,
            amount = 1500m,
            currency = 2,
            source = "John",
            recipient = "Me",
            borrowedAt = "2026-01-01T00:00:00+00:00",
            deadlineAt = (string?)null,
            note = (string?)null,
            capitalId = (int?)null,
            isPaid = true,
            payFromCapitalId = (int?)null
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDebt_ReturnsSuccess()
    {
        (int id, _) = await CreateDebtAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/debts/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
