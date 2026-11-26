using Deed.Application.BankNotifications.Import;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class BankTransactionImporterTests
{
    private readonly ICapitalRepository _capitals = Substitute.For<ICapitalRepository>();
    private readonly IExpenseRepository _expenses = Substitute.For<IExpenseRepository>();
    private readonly IIncomeRepository _incomes = Substitute.For<IIncomeRepository>();
    private readonly BankTransactionImporter _sut;
    private readonly Capital _capital = new(1) { Name = "Card", Balance = 1000m, Currency = CurrencyType.UAH };
    private readonly BankConnection _connection = new(7)
    {
        EncryptedToken = "enc",
        AccountIban = "UA1",
        CapitalId = 1,
        ExpenseCategoryId = 5,
        IncomeCategoryId = 6
    };

    public BankTransactionImporterTests()
    {
        _capitals.GetAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(_capital);

        _sut = new BankTransactionImporter(_capitals, _expenses, _incomes);
    }

    [Fact]
    public async Task ImportAsync_Outgoing_CreatesExpenseAndReducesBalance()
    {
        // Arrange
        var notification = Notification(BankTransactionDirection.Outgoing, 150m);

        // Act
        var result = await _sut.ImportAsync(_connection, notification, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _expenses.Received(1).Create(Arg.Is<Expense>(e =>
            e.Amount == 150m && e.CapitalId == 1 && e.CategoryId == 5 && e.CreatedBy == "owner" && e.Purpose == "Silpo - Groceries"));
        _incomes.DidNotReceive().Create(Arg.Any<Income>());
        _capital.Balance.Should().Be(850m);
        notification.ImportStatus.Should().Be(BankImportStatus.Imported);
    }

    [Fact]
    public async Task ImportAsync_Incoming_CreatesIncomeAndIncreasesBalance()
    {
        // Arrange
        var notification = Notification(BankTransactionDirection.Incoming, 200m);

        // Act
        var result = await _sut.ImportAsync(_connection, notification, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _incomes.Received(1).Create(Arg.Is<Income>(i => i.Amount == 200m && i.CategoryId == 6));
        _capital.Balance.Should().Be(1200m);
    }

    [Fact]
    public async Task ImportAsync_ExplicitCategory_OverridesDefault()
    {
        // Arrange
        var notification = Notification(BankTransactionDirection.Outgoing, 10m);

        // Act
        await _sut.ImportAsync(_connection, notification, 9, CancellationToken.None);

        // Assert
        _expenses.Received(1).Create(Arg.Is<Expense>(e => e.CategoryId == 9));
    }

    [Fact]
    public async Task ImportAsync_CurrencyDiffers_FailsWithoutChanges()
    {
        // Arrange
        var notification = Notification(BankTransactionDirection.Outgoing, 10m, "USD");

        // Act
        var result = await _sut.ImportAsync(_connection, notification, null, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CurrencyMismatch);
        _expenses.DidNotReceive().Create(Arg.Any<Expense>());
        _capital.Balance.Should().Be(1000m);
        notification.ImportStatus.Should().Be(BankImportStatus.Pending);
    }

    [Fact]
    public async Task ImportAsync_NoCapitalLinked_Fails()
    {
        // Arrange
        _connection.CapitalId = null;

        // Act
        var result = await _sut.ImportAsync(_connection, Notification(BankTransactionDirection.Outgoing, 10m), null, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CapitalNotLinked);
    }

    [Fact]
    public async Task ImportAsync_NoCategory_Fails()
    {
        // Arrange
        _connection.ExpenseCategoryId = null;

        // Act
        var result = await _sut.ImportAsync(_connection, Notification(BankTransactionDirection.Outgoing, 10m), null, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CategoryRequired);
    }

    [Fact]
    public async Task ImportAsync_SavingsOnlyCapitalAndExpense_Fails()
    {
        // Arrange
        _capital.OnlyForSavings = true;

        // Act
        var result = await _sut.ImportAsync(_connection, Notification(BankTransactionDirection.Outgoing, 10m), null, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Capital.ForSavingsOnly);
    }

    [Fact]
    public async Task ImportAsync_AlreadyImported_Fails()
    {
        // Arrange
        var notification = Notification(BankTransactionDirection.Outgoing, 10m);
        notification.ImportStatus = BankImportStatus.Imported;

        // Act
        var result = await _sut.ImportAsync(_connection, notification, null, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotPending);
        _expenses.DidNotReceive().Create(Arg.Any<Expense>());
    }

    private static BankNotification Notification(BankTransactionDirection direction, decimal amount, string currency = "UAH")
        => new()
        {
            Kind = BankNotificationKind.Transaction,
            Direction = direction,
            Amount = amount,
            Currency = currency,
            Counterparty = "Silpo",
            Purpose = "Groceries",
            ExternalId = "tx-1",
            OccurredAt = new DateTimeOffset(2026, 11, 26, 10, 0, 0, TimeSpan.Zero),
            ImportStatus = BankImportStatus.Pending,
            CreatedBy = "owner"
        };
}
