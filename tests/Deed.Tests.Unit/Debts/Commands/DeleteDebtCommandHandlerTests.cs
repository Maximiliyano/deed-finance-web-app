using Deed.Application.Auth;
using Deed.Application.Debts.Commands.Delete;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class DeleteDebtCommandHandlerTests
{
    private readonly IDebtRepository _repositoryMock = Substitute.For<IDebtRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteDebtCommandHandler _handler;

    public DeleteDebtCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DeleteDebtCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
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
        var command = new DeleteDebtCommand(1);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns(debt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Delete(debt);
        await _repositoryMock.Received(1).GetAsync(Arg.Any<DebtByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDebtNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteDebtCommand(99);

        _repositoryMock.GetAsync(Arg.Any<DebtByIdSpecification>()).Returns((Debt?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("debt"));

        _repositoryMock.Received(0).Delete(Arg.Any<Debt>());
        await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
