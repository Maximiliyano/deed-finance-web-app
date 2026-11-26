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

public sealed class PrivatAutoclientServiceTests
{
    private const string Iban = "UA213223130000026007233566001";
    private static readonly DateTimeOffset From = new(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetTransactionsAsync_ValidResponse_MapsTransactionsAndSendsTokenHeader()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        var service = CreateService((request, _) =>
        {
            captured = request;
            return Task.FromResult(Json("""
                {
                  "status": "SUCCESS",
                  "exist_next_page": false,
                  "transactions": [
                    { "ID": 1001, "SUM": "150.50", "CCY": "UAH", "TRANTYPE": "D", "AUT_CNTR_NAM": "John", "OSND": "Rent", "DAT_OD": "26.11.2026", "TIM_P": "09:30" },
                    { "REF": "R1", "REFN": "2", "SUM": "20,00", "CCY": "USD", "TRANTYPE": "C", "DATE_TIME_DAT_OD_TIM_P": "26.11.2026 10:15:00" },
                    { "ID": "bad", "SUM": "1", "CCY": "UAH", "TRANTYPE": "X" }
                  ]
                }
                """));
        });

        // Act
        var result = await service.GetTransactionsAsync("secret-token", Iban, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var first = result.Value[0];
        first.ExternalId.Should().Be("1001");
        first.Direction.Should().Be(BankTransactionDirection.Outgoing);
        first.Amount.Should().Be(150.50m);
        first.Counterparty.Should().Be("John");
        first.Purpose.Should().Be("Rent");

        var second = result.Value[1];
        second.ExternalId.Should().Be("R1.2");
        second.Direction.Should().Be(BankTransactionDirection.Incoming);
        second.Amount.Should().Be(20m);

        captured!.Headers.GetValues("token").Should().ContainSingle("secret-token");
        captured.RequestUri!.Query.Should().Contain($"acc={Iban}").And.Contain("startDate=25-11-2026");
        captured.Headers.UserAgent.ToString().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTransactionsAsync_NoIban_RequestsAllAccounts()
    {
        // Arrange
        Uri? captured = null;
        var service = CreateService((request, _) =>
        {
            captured = request.RequestUri;
            return Task.FromResult(Json("""{ "exist_next_page": false, "transactions": [] }"""));
        });

        // Act
        var result = await service.GetTransactionsAsync("t", string.Empty, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        captured!.Query.Should().NotContain("acc=").And.Contain("startDate=25-11-2026");
    }

    [Fact]
    public async Task GetTransactionsAsync_Windows1251Response_DecodesCyrillic()
    {
        // Arrange
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var body = System.Text.Encoding.GetEncoding(1251).GetBytes(
            """{ "exist_next_page": false, "transactions": [ { "ID": "1", "SUM": "5", "CCY": "UAH", "TRANTYPE": "D", "AUT_CNTR_NAM": "Сільпо" } ] }""");
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(body)
        }));

        // Act
        var result = await service.GetTransactionsAsync("t", string.Empty, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Single().Counterparty.Should().Be("Сільпо");
    }

    [Fact]
    public async Task GetTransactionsAsync_FollowsPagination()
    {
        // Arrange
        var calls = 0;
        var service = CreateService((request, _) =>
        {
            calls++;
            var body = request.RequestUri!.Query.Contains("followId=page2")
                ? """{ "exist_next_page": false, "transactions": [ { "ID": "b", "SUM": "2", "CCY": "UAH", "TRANTYPE": "C" } ] }"""
                : """{ "exist_next_page": true, "next_page_id": "page2", "transactions": [ { "ID": "a", "SUM": "1", "CCY": "UAH", "TRANTYPE": "C" } ] }""";

            return Task.FromResult(Json(body));
        });

        // Act
        var result = await service.GetTransactionsAsync("t", Iban, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Select(t => t.ExternalId).Should().Equal("a", "b");
        calls.Should().Be(2);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetTransactionsAsync_AuthFailure_ReturnsTokenInvalid(HttpStatusCode status)
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(status)));

        // Act
        var result = await service.GetTransactionsAsync("t", Iban, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);
    }

    [Fact]
    public async Task GetTransactionsAsync_ServerError_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        // Act
        var result = await service.GetTransactionsAsync("t", Iban, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    [Fact]
    public async Task GetTransactionsAsync_HttpException_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => throw new HttpRequestException("boom"));

        // Act
        var result = await service.GetTransactionsAsync("t", Iban, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    private static PrivatAutoclientService CreateService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new MockHttpMessageHandler(sendAsync);
#pragma warning disable CA2000
        var client = new HttpClient(handler);
#pragma warning restore CA2000
        handler.Dispose();

        return new PrivatAutoclientService(client, Options.Create(new PrivatAutoclientSettings { PageDelayMs = 0 }));
    }

    private static HttpResponseMessage Json(string body)
        => new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}
