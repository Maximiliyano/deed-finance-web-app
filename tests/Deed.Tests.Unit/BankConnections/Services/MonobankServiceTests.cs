using System.Net;
using System.Text;
using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Tests.Common.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Deed.Tests.Unit.BankConnections.Services;

public sealed class MonobankServiceTests
{
    private static readonly DateTimeOffset From = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 11, 26, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetTransactionsAsync_ValidResponse_MapsItemsAndSendsTokenHeader()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        var service = CreateService((request, _) =>
        {
            captured = request;
            return Task.FromResult(Json("""
                [
                  { "id": "a1", "time": 1795680000, "description": "Silpo", "amount": -15050, "currencyCode": 980 },
                  { "id": "b2", "time": 1795683600, "description": "From: John", "comment": "Rent", "amount": 200000, "currencyCode": 980, "counterName": "John" },
                  { "id": "c3", "time": 1795683600, "description": "Zero", "amount": 0, "currencyCode": 980 }
                ]
                """));
        });

        // Act
        var result = await service.GetTransactionsAsync("secret", "acc1", From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var expense = result.Value[0];
        expense.ExternalId.Should().Be("mono:a1");
        expense.Direction.Should().Be(BankTransactionDirection.Outgoing);
        expense.Amount.Should().Be(150.50m);
        expense.Currency.Should().Be("UAH");
        expense.Counterparty.Should().Be("Silpo");
        expense.Purpose.Should().BeNull();

        var income = result.Value[1];
        income.Direction.Should().Be(BankTransactionDirection.Incoming);
        income.Amount.Should().Be(2000m);
        income.Counterparty.Should().Be("John");
        income.Purpose.Should().Be("Rent");

        captured!.Headers.GetValues("X-Token").Should().ContainSingle("secret");
        captured.RequestUri!.AbsolutePath.Should().Be($"/personal/statement/acc1/{From.ToUnixTimeSeconds()}/{To.ToUnixTimeSeconds()}");
    }

    [Fact]
    public async Task GetAccountsAsync_ValidResponse_MapsAccounts()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(Json("""
            {
              "clientId": "x",
              "accounts": [
                { "id": "acc1", "balance": 123456, "type": "black", "currencyCode": 840, "maskedPan": ["537541******1234"], "iban": "UA213223130000026007233566001" }
              ]
            }
            """)));

        // Act
        var result = await service.GetAccountsAsync("secret", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var account = result.Value.Should().ContainSingle().Subject;
        account.Id.Should().Be("acc1");
        account.Currency.Should().Be("USD");
        account.Balance.Should().Be(1234.56m);
        account.MaskedCard.Should().Be("537541******1234");
        account.Iban.Should().Be("UA213223130000026007233566001");
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task GetTransactionsAsync_Rejected_ReturnsTokenInvalid(HttpStatusCode status)
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(status)));

        // Act
        var result = await service.GetTransactionsAsync("t", "acc1", From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);
    }

    [Fact]
    public async Task GetTransactionsAsync_TooManyRequests_ReturnsRateLimited()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        // Act
        var result = await service.GetTransactionsAsync("t", "acc1", From, To, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.RateLimited);
    }

    [Fact]
    public async Task GetTransactionsAsync_HttpException_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => throw new HttpRequestException("boom"));

        // Act
        var result = await service.GetTransactionsAsync("t", "acc1", From, To, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    private static MonobankService CreateService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new MockHttpMessageHandler(sendAsync);
#pragma warning disable CA2000
        var client = new HttpClient(handler);
#pragma warning restore CA2000
        handler.Dispose();

        return new MonobankService(client, Options.Create(new MonobankPersonalSettings()));
    }

    private static HttpResponseMessage Json(string body)
        => new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}
