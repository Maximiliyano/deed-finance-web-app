using Deed.Application.Auth;
using Deed.Application.BankConnections.Queries.GetBankAccounts;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Queries;

public sealed class GetBankAccountsQueryHandlerTests
{
    private const string IbanOne = "UA213223130000026007233566001";
    private const string IbanTwo = "UA213223130000026007233566002";

    [Fact]
    public async Task Handle_MarksConnectedAccountsWithTheirConnection()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Id.Returns("owner");
        var accounts = Substitute.For<IBankAccountRepository>();
        accounts
            .GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankAccount>
            {
                new(1) { Iban = IbanOne, Provider = BankProvider.PrivatAutoclient, Currency = "UAH", Balance = 10m },
                new(2) { Iban = IbanTwo, Provider = BankProvider.PrivatAutoclient, Currency = "USD" }
            });
        var connections = Substitute.For<IBankConnectionRepository>();
        connections
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankConnection>
            {
                new(9) { Provider = BankProvider.PrivatAutoclient, EncryptedToken = "x", AccountIban = "ua213223130000026007233566001" }
            });
        var handler = new GetBankAccountsQueryHandler(user, accounts, connections);

        // Act
        var result = await handler.Handle(new GetBankAccountsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].ConnectionId.Should().Be(9);
        result.Value[0].MaskedAccount.Should().StartWith("UA21").And.EndWith("6001");
        result.Value[1].ConnectionId.Should().BeNull();
    }
}
