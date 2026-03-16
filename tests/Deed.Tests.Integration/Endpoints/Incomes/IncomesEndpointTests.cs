using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Incomes;

public sealed class IncomesEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
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

    private async Task<int> CreateCategoryAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Cat_{Guid.NewGuid().ToString("N")[..8]}",
            type = 2,
            plannedPeriodAmount = 0m,
            period = 0
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    private async Task<int> CreateIncomeAsync()
    {
        int capitalId = await CreateCapitalAsync();
        int categoryId = await CreateCategoryAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/incomes", new
        {
            capitalId,
            categoryId,
            amount = 500m,
            paymentDate = "2026-01-01T00:00:00+00:00",
            purpose = "Salary",
            tags = new string[0]
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateIncome_ReturnsId()
    {
        int id = await CreateIncomeAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllIncomes_ReturnsResults()
    {
        await CreateIncomeAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/incomes");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteIncome_ReturnsSuccess()
    {
        int id = await CreateIncomeAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/incomes/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
