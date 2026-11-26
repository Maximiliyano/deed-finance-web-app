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

public sealed class PrivatCardServiceTests
{
    private const string Card = "4111111111111111";
    private static readonly PrivatCardCredentials Credentials = new("12345", "pass");
    private static readonly DateTimeOffset From = new(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetTransactionsAsync_ValidResponse_MapsStatementsAndSendsSignedRequest()
    {
        // Arrange
        string? sentBody = null;
        var service = CreateService(async (request, ct) =>
        {
            sentBody = await request.Content!.ReadAsStringAsync(ct);
            return Xml("""
                <response version="1.0"><merchant><id>12345</id></merchant><data><oper>cmt</oper><info>
                <statements status="excellent" credit="0" debet="0">
                <statement card="4111111111111111" appcode="111" trandate="2026-11-26" trantime="09:30:00" amount="-150.50 UAH" terminal="SILPO" description="Purchase" />
                <statement card="4111111111111111" appcode="222" trandate="2026-11-26" trantime="10:15:00" amount="20.00 USD" terminal="" description="Transfer" />
                <statement card="4111111111111111" amount="oops" />
                </statements></info></data></response>
                """);
        });

        // Act
        var result = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var first = result.Value[0];
        first.Direction.Should().Be(BankTransactionDirection.Outgoing);
        first.Amount.Should().Be(150.50m);
        first.Currency.Should().Be("UAH");
        first.Counterparty.Should().Be("SILPO");
        first.Purpose.Should().Be("Purchase");

        var second = result.Value[1];
        second.Direction.Should().Be(BankTransactionDirection.Incoming);
        second.Currency.Should().Be("USD");
        second.Counterparty.Should().BeNull();
        second.ExternalId.Should().NotBe(first.ExternalId);

        sentBody.Should().Contain("<id>12345</id>").And.Contain("name=\"sd\" value=\"25.11.2026\"").And.Contain($"name=\"card\" value=\"{Card}\"");
        sentBody.Should().NotContain("pass");
    }

    [Fact]
    public async Task GetTransactionsAsync_SameStatementTwice_ProducesSameExternalId()
    {
        // Arrange
        const string body = """
            <response><data><info><statements>
            <statement card="4111111111111111" appcode="111" trandate="2026-11-26" trantime="09:30:00" amount="-5.00 UAH" terminal="T" description="D" />
            </statements></info></data></response>
            """;
        var service = CreateService((_, _) => Task.FromResult(Xml(body)));

        // Act
        var first = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);
        var second = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        first.Value[0].ExternalId.Should().Be(second.Value[0].ExternalId);
    }

    [Fact]
    public async Task GetTransactionsAsync_SignatureError_ReturnsTokenInvalid()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(Xml("""<response><data><error message="Invalid signature" /></data></response>""")));

        // Act
        var result = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);
    }

    [Fact]
    public async Task GetTransactionsAsync_OtherError_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(Xml("""<response><data><error message="Service is busy" /></data></response>""")));

        // Act
        var result = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    [Fact]
    public async Task GetTransactionsAsync_ServerError_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        // Act
        var result = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    [Fact]
    public async Task GetTransactionsAsync_MalformedXml_ReturnsExecutionFailure()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(Xml("<response><data>")));

        // Act
        var result = await service.GetTransactionsAsync(Credentials, Card, From, To, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.HttpClient.Execution);
    }

    [Fact]
    public void Sign_ComputesSha1OfMd5()
    {
        // Act
        var signature = PrivatCardService.Sign("a", "b");

        // Assert
        signature.Should().Be("f695998215845af35e73d7d2cefa559d103d12e3");
    }

    private static PrivatCardService CreateService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new MockHttpMessageHandler(sendAsync);
#pragma warning disable CA2000
        var client = new HttpClient(handler);
#pragma warning restore CA2000
        handler.Dispose();

        return new PrivatCardService(client, Options.Create(new PrivatCardSettings()));
    }

    private static HttpResponseMessage Xml(string body)
        => new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "text/xml") };
}
