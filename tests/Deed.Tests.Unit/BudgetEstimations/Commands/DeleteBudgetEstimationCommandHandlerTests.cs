using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Commands.Delete;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BudgetEstimations.Commands;

public sealed class DeleteBudgetEstimationCommandHandlerTests
{
    private readonly IBudgetEstimationRepository _repositoryMock = Substitute.For<IBudgetEstimationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteBudgetEstimationCommandHandler _handler;

    public DeleteBudgetEstimationCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DeleteBudgetEstimationCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenEstimationExists_ReturnsSuccess()
    {
        // Arrange
        var estimation = new BudgetEstimation(1)
        {
            Description = "Groceries",
            BudgetAmount = 500m,
            BudgetCurrency = CurrencyType.UAH
        };
        var command = new DeleteBudgetEstimationCommand(1);

        _repositoryMock.GetAsync(Arg.Any<BudgetEstimationByIdSpecification>()).Returns(estimation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Delete(estimation);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEstimationNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteBudgetEstimationCommand(99);
        _repositoryMock.GetAsync(Arg.Any<BudgetEstimationByIdSpecification>()).Returns((BudgetEstimation?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("budget estimation"));
        _repositoryMock.Received(0).Delete(Arg.Any<BudgetEstimation>());
        await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
