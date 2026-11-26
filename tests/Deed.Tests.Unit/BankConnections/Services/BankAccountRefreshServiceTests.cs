using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Services;

public sealed class BankAccountRefreshServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string IbanOne = "UA213223130000026007233566001";
    private const string IbanTwo = "UA213223130000026007233566002";

    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IBankAccountRepository _accountsMock = Substitute.For<IBankAccountRepository>();
    private readonly IPrivatAutoclientService _bankMock = Substitute.For<IPrivatAutoclientService>();
    private readonly ITokenProtector _protectorMock = Substitute.For<ITokenProtector>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly BankAccountRefreshService _service;

    public BankAccountRefreshServiceTests()
    {
        _clockMock.UtcNow.Returns(Now);
        _protectorMock.Unprotect(Arg.Any<string>()).Returns(call => call.Arg<string>().Replace("enc:", string.Empty, StringComparison.Ordinal));
        _accountsMock
            .GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankAccount>());

        _service = new BankAccountRefreshService(_connectionsMock, _accountsMock, _bankMock, _protectorMock, _clockMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task RefreshAsync_NewAccounts_StoresThemOnce()
    {
        // Arrange
        ReturnConnections(Connection(1, "enc:secret", IbanOne), Connection(2, "enc:secret", IbanTwo));
        ReturnAccounts("secret", Account(IbanOne, 100m), Account(IbanTwo, 5m));

        // Act
        var refreshed = await _service.RefreshAsync(CancellationToken.None);

        // Assert
        refreshed.Should().Be(2);
        await _bankMock.Received(1).GetAccountsAsync("secret", Arg.Any<CancellationToken>());
        _accountsMock.Received(1).Create(Arg.Is<BankAccount>(a =>
            a.Iban == IbanOne && a.Balance == 100m && a.CreatedBy == "owner" && a.SourceConnectionId == 1 && a.LastRefreshedAt == Now));
        _accountsMock.Received(1).Create(Arg.Is<BankAccount>(a => a.Iban == IbanTwo));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_KnownAccount_UpdatesBalanceAndName()
    {
        // Arrange
        var row = new BankAccount(7) { Iban = IbanOne, Balance = 1m, CreatedBy = "owner", SourceConnectionId = 9 };
        ReturnConnections(Connection(1, "enc:secret", IbanOne));
        _accountsMock
            .GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankAccount> { row });
        ReturnAccounts("secret", new PrivatAccount(IbanOne, "UAH", "Main", 250.5m));

        // Act
        await _service.RefreshAsync(CancellationToken.None);

        // Assert
        row.Balance.Should().Be(250.5m);
        row.Name.Should().Be("Main");
        row.Currency.Should().Be("UAH");
        row.SourceConnectionId.Should().Be(1);
        _accountsMock.DidNotReceive().Create(Arg.Any<BankAccount>());
    }

    [Fact]
    public async Task RefreshAsync_AccountGoneFromBank_RemovesOnlyUnconnectedOnes()
    {
        // Arrange
        var connectedRow = new BankAccount(7) { Iban = IbanOne, CreatedBy = "owner" };
        var goneRow = new BankAccount(8) { Iban = "UA999999999999999999999999999", CreatedBy = "owner" };
        ReturnConnections(Connection(1, "enc:secret", IbanOne));
        _accountsMock
            .GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankAccount> { connectedRow, goneRow });
        ReturnAccounts("secret", Account(IbanTwo, 1m));

        // Act
        await _service.RefreshAsync(CancellationToken.None);

        // Assert
        _accountsMock.Received(1).Delete(goneRow);
        _accountsMock.DidNotReceive().Delete(connectedRow);
    }

    [Fact]
    public async Task RefreshAsync_BankFails_KeepsExistingAccounts()
    {
        // Arrange
        var row = new BankAccount(7) { Iban = IbanTwo, CreatedBy = "owner" };
        ReturnConnections(Connection(1, "enc:secret", IbanOne));
        _accountsMock
            .GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankAccount> { row });
        _bankMock
            .GetAccountsAsync("secret", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<PrivatAccount>>(DomainErrors.HttpClient.Execution));

        // Act
        var refreshed = await _service.RefreshAsync(CancellationToken.None);

        // Assert
        refreshed.Should().Be(0);
        _accountsMock.DidNotReceive().Delete(Arg.Any<BankAccount>());
    }

    [Fact]
    public async Task RefreshAsync_AsksOnlyForActivePrivatConnectionsIncludingExcludedOnes()
    {
        // Arrange
        ISpecification<BankConnection>? captured = null;
        _connectionsMock
            .GetAllAsync(Arg.Do<ISpecification<BankConnection>>(s => captured = s), Arg.Any<CancellationToken>())
            .Returns(new List<BankConnection>());
        var privat = Connection(1, "enc:secret", IbanOne);
        var excluded = Connection(2, "enc:secret", IbanTwo);
        excluded.IsExcluded = true;
        var monobank = Connection(3, "enc:secret", "UA3");
        monobank.Provider = BankProvider.Monobank;

        // Act
        await _service.RefreshAsync(CancellationToken.None);

        // Assert
        var matches = captured!.Criteria!.Compile();
        matches(privat).Should().BeTrue();
        matches(excluded).Should().BeTrue();
        matches(monobank).Should().BeFalse();
    }

    [Fact]
    public async Task RefreshUserAsync_UndecryptableToken_DoesNotCallBank()
    {
        // Arrange
        ReturnConnections(Connection(1, "enc:secret", IbanOne));
        _protectorMock.Unprotect(Arg.Any<string>()).Returns((string?)null);

        // Act
        var refreshed = await _service.RefreshUserAsync("owner", CancellationToken.None);

        // Assert
        refreshed.Should().Be(0);
        await _bankMock.DidNotReceive().GetAccountsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void ReturnConnections(params BankConnection[] connections)
        => _connectionsMock
            .GetAllAsync(Arg.Any<ActiveBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(connections);

    private void ReturnAccounts(string token, params PrivatAccount[] accounts)
        => _bankMock
            .GetAccountsAsync(token, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PrivatAccount>>(accounts));

    private static PrivatAccount Account(string iban, decimal balance)
        => new(iban, "UAH", null, balance);

    private static BankConnection Connection(int id, string encryptedToken, string iban)
        => new(id)
        {
            Provider = BankProvider.PrivatAutoclient,
            EncryptedToken = encryptedToken,
            AccountIban = iban,
            Label = "Business",
            Status = BankConnectionStatus.Active,
            CreatedBy = "owner"
        };
}
