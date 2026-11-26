using Deed.Application.Auth;
using Deed.Application.BankConnections.Queries.GetAll;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Queries;

public sealed class GetBankConnectionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_MapsConnectionsIncludingTheNotCountedFlag()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");
        var repository = Substitute.For<IBankConnectionRepository>();
        repository
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA213223130000026007233566001", Provider = BankProvider.PrivatAutoclient, Label = "Main", Status = BankConnectionStatus.Active },
                new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2", Provider = BankProvider.Monobank, Label = "Mono", Status = BankConnectionStatus.TokenInvalid, IsExcluded = true }
            ]);
        var handler = new GetBankConnectionsQueryHandler(user, repository);

        // Act
        var result = await handler.Handle(new GetBankConnectionsQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
        result.Value[0].MaskedAccount.Should().Be("UA21" + new string('*', 21) + "6001");
        result.Value[0].IsExcluded.Should().BeFalse();
        result.Value[1].Status.Should().Be("TokenInvalid");
        result.Value[1].IsExcluded.Should().BeTrue();
    }
}
