using Deed.Application.Auth;
using Deed.Application.Debts;
using Deed.Application.Debts.Queries.GetAll;
using Deed.Application.Debts.Responses;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Queries;

public sealed class GetAllDebtsQueryHandlerTests
{
    private readonly IDebtRepository _repositoryMock = Substitute.For<IDebtRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetAllDebtsQueryHandler _handler;

    public GetAllDebtsQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetAllDebtsQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ReturnsDebts_WhenDebtsExist()
    {
        // Arrange
        var query = new GetAllDebtsQuery();
        var debts = new List<Debt>
        {
            new(1)
            {
                Item = "Laptop",
                Amount = 1500m,
                Currency = CurrencyType.USD,
                Source = "John",
                Recipient = "Me",
                BorrowedAt = DateTimeOffset.UtcNow
            }
        };
        var expected = debts.ToResponses();

        _repositoryMock.GetAllAsync(Arg.Any<DebtsByUserSpecification>()).Returns(debts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);

        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<DebtsByUserSpecification>());
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoDebts()
    {
        // Arrange
        var query = new GetAllDebtsQuery();

        _repositoryMock.GetAllAsync(Arg.Any<DebtsByUserSpecification>()).Returns(new List<Debt>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(Enumerable.Empty<DebtResponse>());

        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<DebtsByUserSpecification>());
    }
}
