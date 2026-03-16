using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Commands.Update;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BudgetEstimations.Commands;

public sealed class UpdateBudgetEstimationCommandHandlerTests
{
    private readonly IBudgetEstimationRepository _repositoryMock = Substitute.For<IBudgetEstimationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateBudgetEstimationCommandHandler _handler;

    public UpdateBudgetEstimationCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpdateBudgetEstimationCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenEstimationExists_ReturnsSuccess()
    {
        // Arrange
        var estimation = new BudgetEstimation(1)
        {
            Description = "Old",
            BudgetAmount = 500m,
            BudgetCurrency = CurrencyType.UAH
        };
        var command = new UpdateBudgetEstimationCommand(
            1, "New Description", 1000m, CurrencyType.USD, 2);

        _repositoryMock.GetAsync(Arg.Any<BudgetEstimationByIdSpecification>()).Returns(estimation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        estimation.Description.Should().Be("New Description");
        estimation.BudgetAmount.Should().Be(1000m);
        estimation.BudgetCurrency.Should().Be(CurrencyType.USD);
        estimation.CapitalId.Should().Be(2);

        _repositoryMock.Received(1).Update(estimation);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEstimationNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateBudgetEstimationCommand(
            99, "Desc", 100m, CurrencyType.UAH, null);

        _repositoryMock.GetAsync(Arg.Any<BudgetEstimationByIdSpecification>()).Returns((BudgetEstimation?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("budget estimation"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<BudgetEstimation>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
