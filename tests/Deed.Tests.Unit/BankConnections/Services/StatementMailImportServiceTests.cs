using System.Text;
using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankConnections.Statements;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Services;

public sealed class StatementMailImportServiceTests
{
    private const string Iban = "UA213223130000026007233566001";
    private const string Key = "A1B2C3D4E5F60718";
    private const string Passing = "mx.google.com; dkim=pass header.i=@privatbank.ua";

    private readonly IStatementFileParser _parserMock = Substitute.For<IStatementFileParser>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IBankTransactionSyncService _syncMock = Substitute.For<IBankTransactionSyncService>();

    private readonly BankConnection _connection;
    private readonly StatementMailImportService _service;

    public StatementMailImportServiceTests()
    {
        _connection = new BankConnection(7)
        {
            Provider = BankProvider.PrivatMail,
            EncryptedToken = string.Empty,
            AccountIban = Iban,
            ProviderAccountId = Key,
            Currency = "UAH",
            Status = BankConnectionStatus.Active,
            CreatedBy = "owner"
        };

        _connectionsMock
            .GetAllAsync(Arg.Any<MailBankConnectionsByKeysSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);

        _syncMock
            .ImportStatementAsync(Arg.Any<BankConnection>(), Arg.Any<IReadOnlyCollection<BankTransaction>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyCollection<BankTransaction>>().Count);

        var settings = Options.Create(new StatementMailSettings { Host = "imap.example.org", UserName = "u", Password = "p", Address = "deed.statements@gmail.com" });

        _service = new StatementMailImportService(settings, _parserMock, _connectionsMock, _syncMock);
    }

    [Fact]
    public async Task ImportAsync_UntrustedSender_IsRejectedWithoutTouchingTheDatabase()
    {
        // Arrange
        var mail = Mail(from: "attacker@evil.com");

        // Act
        var result = await _service.ImportAsync(mail, CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(StatementMailOutcome.Rejected);
        await _connectionsMock.DidNotReceive().GetAllAsync(Arg.Any<MailBankConnectionsByKeysSpecification>(), Arg.Any<CancellationToken>());
        await _syncMock.DidNotReceive().ImportStatementAsync(Arg.Any<BankConnection>(), Arg.Any<IReadOnlyCollection<BankTransaction>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_NoKeyInRecipients_IsUnrouted()
    {
        // Arrange
        var mail = Mail(recipients: ["deed.statements@gmail.com"]);

        // Act
        var result = await _service.ImportAsync(mail, CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(StatementMailOutcome.Unrouted);
        await _connectionsMock.DidNotReceive().GetAllAsync(Arg.Any<MailBankConnectionsByKeysSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_UnknownKey_IsUnrouted()
    {
        // Arrange
        _connectionsMock
            .GetAllAsync(Arg.Any<MailBankConnectionsByKeysSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<BankConnection>());

        // Act
        var result = await _service.ImportAsync(Mail(), CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(StatementMailOutcome.Unrouted);
    }

    [Fact]
    public async Task ImportAsync_UnparseableAttachments_AreUnreadable()
    {
        // Arrange
        _parserMock.Parse(Arg.Any<StatementAttachment>()).Returns((ParsedStatement?)null);

        // Act
        var result = await _service.ImportAsync(Mail(), CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(StatementMailOutcome.Unreadable);
    }

    [Fact]
    public async Task ImportAsync_ValidStatement_ImportsRowsIntoTheConnectionOfTheKey()
    {
        // Arrange
        ReturnStatement(Rows(Row(-100m, "Silpo"), Row(500m, "Salary")));

        // Act
        var result = await _service.ImportAsync(Mail(), CancellationToken.None);

        // Assert
        result.Outcome.Should().Be(StatementMailOutcome.Imported);
        result.Created.Should().Be(2);
        await _syncMock.Received(1).ImportStatementAsync(
            _connection,
            Arg.Is<IReadOnlyCollection<BankTransaction>>(items =>
                items.Count == 2 &&
                items.First().Direction == BankTransactionDirection.Outgoing &&
                items.First().Amount == 100m &&
                items.First().Currency == "UAH" &&
                items.First().Counterparty == "Silpo" &&
                items.Last().Direction == BankTransactionDirection.Incoming),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_StatementOfAnotherAccount_IsSkipped()
    {
        // Arrange
        ReturnStatement(new ParsedStatement(Rows(Row(-100m, "Silpo")), ["UA000000000000000000000000000"], string.Empty));

        // Act
        var result = await _service.ImportAsync(Mail(), CancellationToken.None);

        // Assert
        result.Created.Should().Be(0);
        await _syncMock.DidNotReceive().ImportStatementAsync(Arg.Any<BankConnection>(), Arg.Any<IReadOnlyCollection<BankTransaction>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_StatementNamingTheSameAccount_IsImported()
    {
        // Arrange
        ReturnStatement(new ParsedStatement(Rows(Row(-100m, "Silpo")), [Iban], string.Empty));

        // Act
        var result = await _service.ImportAsync(Mail(), CancellationToken.None);

        // Assert
        result.Created.Should().Be(1);
    }

    [Fact]
    public void ToTransactions_IdenticalRowsInOneStatement_GetDistinctStableIds()
    {
        // Arrange
        var statement = new ParsedStatement(Rows(Row(-45m, "Coffee"), Row(-45m, "Coffee")), [], string.Empty);

        // Act
        var first = StatementMailImportService.ToTransactions(_connection, statement);
        var second = StatementMailImportService.ToTransactions(_connection, statement);

        // Assert
        first.Select(t => t.ExternalId).Distinct().Should().HaveCount(2);
        first.Select(t => t.ExternalId).Should().Equal(second.Select(t => t.ExternalId));
    }

    [Fact]
    public void ToTransactions_RowWithoutCounterparty_UsesTheDescriptionAsCounterparty()
    {
        // Arrange
        var statement = new ParsedStatement([new StatementRow(Now, -10m, "EUR", "Card payment", null, null)], [], string.Empty);

        // Act
        var transactions = StatementMailImportService.ToTransactions(_connection, statement);

        // Assert
        transactions.Single().Counterparty.Should().Be("Card payment");
        transactions.Single().Purpose.Should().BeNull();
        transactions.Single().Currency.Should().Be("UAH");
    }

    [Fact]
    public void ToTransactions_RowWithCounterparty_KeepsTheDescriptionAsPurpose()
    {
        // Arrange
        var statement = new ParsedStatement([new StatementRow(Now, 10m, null, "Rent share", "Olena K.", null)], [], string.Empty);

        // Act
        var transactions = StatementMailImportService.ToTransactions(_connection, statement);

        // Assert
        transactions.Single().Counterparty.Should().Be("Olena K.");
        transactions.Single().Purpose.Should().Be("Rent share");
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.FromHours(2));

    private void ReturnStatement(ParsedStatement statement)
        => _parserMock.Parse(Arg.Any<StatementAttachment>()).Returns(statement);

    private void ReturnStatement(IReadOnlyList<StatementRow> rows)
        => ReturnStatement(new ParsedStatement(rows, [], string.Empty));

    private static StatementRow[] Rows(params StatementRow[] rows) => rows;

    private static StatementRow Row(decimal amount, string description)
        => new(Now, amount, "UAH", description, null, null);

    private static StatementMail Mail(string from = "noreply@privatbank.ua", string[]? recipients = null)
        => new(
            from,
            "Statement",
            Passing,
            recipients ?? [$"deed.statements+{Key}@gmail.com"],
            [new StatementAttachment("statement.csv", "text/csv", Encoding.UTF8.GetBytes("x"))],
            null);
}
