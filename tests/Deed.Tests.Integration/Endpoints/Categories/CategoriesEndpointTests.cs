using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Categories;

public sealed class CategoriesEndpointTests(DeedApiFactory factory) : IntegrationTest(factory)
{
    private async Task<int> CreateCategoryAsync(int type = 1)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Cat_{Guid.NewGuid().ToString("N")[..8]}",
            type,
            plannedPeriodAmount = 0m,
            period = 0
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    [Fact]
    public async Task CreateCategory_ReturnsId()
    {
        int id = await CreateCategoryAsync();

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsResults()
    {
        await CreateCategoryAsync();

        HttpResponseMessage response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsSuccess()
    {
        int id = await CreateCategoryAsync();

        HttpResponseMessage response = await Client.DeleteAsync($"/api/categories/{id}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
