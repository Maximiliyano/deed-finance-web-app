using Deed.Application.Auth;
using Deed.Application.Debts.Commands.Update;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class UpdateDebtCommandHandlerTests
{
    private readonly IDebtRepository _repositoryMock = Substitute.For<IDebtRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateDebtCommandHandler _handler;

    public UpdateDebtCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpdateDebtCommandHandler(_repositoryMock, _capitalRepositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenDebtExists_ReturnsSuccess()
    {
        // Arrange
        var debt = new Debt(1)
        {
            Item = "Laptop",
            Amount = 1000m,
            Currency = CurrencyType.USD,
            Source = "John",
            Recipient = "Me",
            BorrowedAt = DateTimeOffset.UtcNow
        };
        var command = new UpdateDebtCommand(
            1, "Updated Item", 2000m, CurrencyType.EUR,
            "Jane", "You", DateTimeOffset.UtcNow,
            null, "Updated note", true, null);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        debt.Item.Should().Be("Updated Item");
        debt.Amount.Should().Be(2000m);
        debt.IsPaid.Should().BeTrue();

        _repositoryMock.Received(1).Update(debt);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDebtNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateDebtCommand(
            99, "Item", 100m, CurrencyType.UAH,
            "A", "B", DateTimeOffset.UtcNow, null, null, false, null);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns((Debt?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("debt"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<Debt>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
