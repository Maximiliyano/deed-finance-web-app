using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Expenses;

public sealed class ExpensesEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
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
            type = 1,
            plannedPeriodAmount = 0m,
            period = 0
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    private async Task<int> CreateExpenseAsync()
    {
        int capitalId = await CreateCapitalAsync();
        int categoryId = await CreateCategoryAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/expenses", new
        {
            capitalId,
            categoryId,
            amount = 50m,
            paymentDate = "2026-01-01T00:00:00+00:00",
            purpose = "Lunch",
            tagNames = new string[0]
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateExpense_ReturnsId()
    {
        int id = await CreateExpenseAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllExpenses_ReturnsResults()
    {
        await CreateExpenseAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/expenses");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteExpense_ReturnsSuccess()
    {
        int id = await CreateExpenseAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/expenses/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
