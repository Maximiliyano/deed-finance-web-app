using System.Net;
using System.Net.Http.Json;
using Deed.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace Deed.Tests.Integration.Endpoints.Bank;

public sealed class BankEndpointsTests(DeedApiFactory factory) : IntegrationTest(factory, "api/bank-connections")
{
    [Fact]
    public async Task GetConnections_NothingConnected_ReturnsEmptyList()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/bank-connections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<List<object>>()).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccounts_NothingStored_ReturnsEmptyList()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/bank-accounts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<List<object>>()).Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAccounts_NothingConnected_ReturnsZero()
    {
        HttpResponseMessage response = await Client.PostAsync("/api/bank-accounts/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<int>()).Should().Be(0);
    }

    [Fact]
    public async Task ConnectAccount_UnknownAccount_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.PostAsync("/api/bank-accounts/999999/connect", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactions_NothingStored_ReturnsEmptyPage()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/bank-transactions?page=1&pageSize=20&provider=Monobank&currency=UAH");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSummary_NothingStored_ReturnsNoCurrencies()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/bank-transactions/summary?period=month");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSummary_UnknownPeriod_ReturnsBadRequest()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/bank-transactions/summary?period=week");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetExcluded_UnknownConnection_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync("/api/bank-connections/999999/excluded", new { excluded = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelHistory_NothingRunning_ReturnsNoContent()
    {
        HttpResponseMessage response = await Client.DeleteAsync("/api/bank-connections/history");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ImportHistory_NothingConnected_IsNotInProgress()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/bank-connections/history", new { from = DateTimeOffset.UtcNow.AddDays(-30), to = (DateTimeOffset?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DisconnectProvider_NothingConnected_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.DeleteAsync("/api/bank-connections/provider/Monobank");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DisconnectProvider_UnknownBank_ReturnsBadRequest()
    {
        HttpResponseMessage response = await Client.DeleteAsync("/api/bank-connections/provider/revolut");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
