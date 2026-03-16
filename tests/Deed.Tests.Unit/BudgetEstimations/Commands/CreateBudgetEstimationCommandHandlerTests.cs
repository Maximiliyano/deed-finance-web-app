using Deed.Application.BudgetEstimations.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BudgetEstimations.Commands;

public sealed class CreateBudgetEstimationCommandHandlerTests
{
    private readonly IBudgetEstimationRepository _repositoryMock = Substitute.For<IBudgetEstimationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateBudgetEstimationCommandHandler _handler;

    public CreateBudgetEstimationCommandHandlerTests()
    {
        _handler = new CreateBudgetEstimationCommandHandler(_repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidEstimation_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateBudgetEstimationCommand(
            "Groceries  ", 500m, CurrencyType.UAH, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Create(Arg.Is<BudgetEstimation>(e =>
            e.Description == "Groceries" &&
            e.BudgetAmount == 500m &&
            e.BudgetCurrency == CurrencyType.UAH &&
            e.CapitalId == 1
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateEstimationWithoutCapital_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateBudgetEstimationCommand(
            "Rent", 1000m, CurrencyType.USD, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Is<BudgetEstimation>(e =>
            e.CapitalId == null
        ));
    }
}
