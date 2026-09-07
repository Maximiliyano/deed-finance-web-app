using System.Net;
using System.Net.Http.Json;
using Deed.Application.Capitals.Requests;
using Deed.Application.Capitals.Responses;
using Deed.Domain.Enums;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Capitals;

public sealed class CapitalsEndpointTests(DeedApiFactory factory)
    : IntegrationTest(factory, "api/capitals")
{
    [Fact]
    public async Task CreateValidCapital_ReturnsId()
    {
        CreateCapitalRequest request = new(
            $"TestRun_{Guid.NewGuid().ToString("N")[..8]}",
            5000m,
            CurrencyType.UAH,
            true,
            false
        );

        int id = await ExecutePostAsync<CreateCapitalRequest, int>(request);

        id.Should().BeGreaterThan(0);
        Assert.NotEqual(0, id);

        CapitalResponse? capital = await ExecuteGetAsync<CapitalResponse>(id.ToString());

        Assert.NotNull(capital);
        Assert.Equal(request.Name, capital.Name);
        Assert.Equal(request.Balance, capital.Balance);
        Assert.Equal(request.Currency, Enum.Parse<CurrencyType>(capital.Currency));
        Assert.Equal(request.IncludeInTotal, capital.IncludeInTotal);
        Assert.Equal(request.OnlyForSavings, capital.OnlyForSavings);
    }

    [Fact]
    public async Task GetAllCapitals_ReturnsResults()
    {
        await CreateCapitalAsync();

        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/capitals/all",
            new
            {
                searchTerm = (string?)null,
                sortBy = (string?)null,
                sortDirection = (string?)null,
                filterBy = (string?)null
            });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCapital_ReturnsSuccess()
    {
        int id = await CreateCapitalAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/capitals/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PatchIncludeInTotal_ReturnsSuccess()
    {
        int id = await CreateCapitalAsync();

        HttpRequestMessage request = new(HttpMethod.Patch,
            $"/api/capitals/{id}/include-in-total")
        {
            Content = JsonContent.Create(false)
        };
        HttpResponseMessage response = await Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    private async Task<int> CreateCapitalAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/capitals",
            new
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
}
