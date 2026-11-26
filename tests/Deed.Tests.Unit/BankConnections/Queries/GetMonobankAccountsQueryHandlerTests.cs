using Deed.Application.BankConnections.Queries.GetMonobankAccounts;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Queries;

public sealed class GetMonobankAccountsQueryHandlerTests
{
    private readonly IMonobankService _bankMock = Substitute.For<IMonobankService>();

    [Fact]
    public async Task Handle_Token_IsTrimmedAndAccountsAreReturned()
    {
        // Arrange
        IReadOnlyList<MonobankAccount> accounts = [new("id", "UA1", "UAH", null, "black", 5m)];
        _bankMock.GetAccountsAsync("secret", Arg.Any<CancellationToken>()).Returns(Result.Success(accounts));
        var handler = new GetMonobankAccountsQueryHandler(_bankMock);

        // Act
        var result = await handler.Handle(new GetMonobankAccountsQuery(" secret "), CancellationToken.None);

        // Assert
        result.Value.Should().Equal(accounts);
    }

    [Fact]
    public async Task Handle_BankRejectsToken_ReturnsTheBankError()
    {
        // Arrange
        _bankMock
            .GetAccountsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<MonobankAccount>>(DomainErrors.Bank.TokenInvalid));
        var handler = new GetMonobankAccountsQueryHandler(_bankMock);

        // Act
        var result = await handler.Handle(new GetMonobankAccountsQuery("bad"), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);
    }
}
