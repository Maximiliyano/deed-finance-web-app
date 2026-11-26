using Deed.Application.BankConnections.Queries.GetPrivatAccounts;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Queries;

public sealed class GetPrivatAccountsQueryHandlerTests
{
    private readonly IPrivatAutoclientService _bankMock = Substitute.For<IPrivatAutoclientService>();

    [Fact]
    public async Task Handle_Token_IsTrimmedAndAccountsAreReturned()
    {
        // Arrange
        IReadOnlyList<PrivatAccount> accounts = [new("UA1", "UAH", "Main", 10m)];
        _bankMock.GetAccountsAsync("secret", Arg.Any<CancellationToken>()).Returns(Result.Success(accounts));
        var handler = new GetPrivatAccountsQueryHandler(_bankMock);

        // Act
        var result = await handler.Handle(new GetPrivatAccountsQuery("  secret  "), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(accounts);
    }
}
