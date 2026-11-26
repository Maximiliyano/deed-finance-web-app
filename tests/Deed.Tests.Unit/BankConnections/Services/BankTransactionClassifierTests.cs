using Deed.Application.BankConnections.Service;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Services;

public sealed class BankTransactionClassifierTests
{
    [Theory]
    [InlineData(BankTransactionDirection.Outgoing, null, "Зняття готівки в банкоматі", BankTransactionType.Withdrawal)]
    [InlineData(BankTransactionDirection.Outgoing, "ATM 123", null, BankTransactionType.Withdrawal)]
    [InlineData(BankTransactionDirection.Outgoing, null, "Cash withdrawal", BankTransactionType.Withdrawal)]
    [InlineData(BankTransactionDirection.Incoming, null, "Поповнення картки", BankTransactionType.TopUp)]
    [InlineData(BankTransactionDirection.Incoming, null, "Внесення готівки", BankTransactionType.TopUp)]
    [InlineData(BankTransactionDirection.Outgoing, null, "Переказ на картку", BankTransactionType.Transfer)]
    [InlineData(BankTransactionDirection.Incoming, "John", "Переказ з картки на картку", BankTransactionType.Transfer)]
    [InlineData(BankTransactionDirection.Outgoing, "SILPO", "Purchase", BankTransactionType.Payment)]
    [InlineData(BankTransactionDirection.Incoming, "Employer", "Salary", BankTransactionType.TopUp)]
    [InlineData(BankTransactionDirection.Outgoing, "ATMOSPHERE CAFE", "Coffee", BankTransactionType.Payment)]
    [InlineData(BankTransactionDirection.Outgoing, null, "Обмін валют UAH/USD", BankTransactionType.Exchange)]
    [InlineData(BankTransactionDirection.Incoming, null, "Конвертація коштів", BankTransactionType.Exchange)]
    [InlineData(BankTransactionDirection.Outgoing, null, "Currency exchange", BankTransactionType.Exchange)]
    public void Classify_UsesDirectionAndDescription(
        BankTransactionDirection direction,
        string? counterparty,
        string? purpose,
        BankTransactionType expected)
    {
        // Act
        var result = BankTransactionClassifier.Classify(direction, counterparty, purpose);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Classify_IncomingWithWithdrawalWording_IsNotWithdrawal()
    {
        // Act
        var result = BankTransactionClassifier.Classify(BankTransactionDirection.Incoming, null, "Зняття готівки");

        // Assert
        result.Should().NotBe(BankTransactionType.Withdrawal);
    }
}
