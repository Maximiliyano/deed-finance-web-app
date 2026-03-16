using Deed.Application.Auth;
using Deed.Application.BudgetEstimations;
using Deed.Application.BudgetEstimations.Queries.GetAll;
using Deed.Application.BudgetEstimations.Responses;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BudgetEstimations.Queries;

public sealed class GetAllBudgetEstimationsQueryHandlerTests
{
    private readonly IBudgetEstimationRepository _repositoryMock = Substitute.For<IBudgetEstimationRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetAllBudgetEstimationsQueryHandler _handler;

    public GetAllBudgetEstimationsQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetAllBudgetEstimationsQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ReturnsEstimations_WhenEstimationsExist()
    {
        // Arrange
        var query = new GetAllBudgetEstimationsQuery();
        var estimations = new List<BudgetEstimation>
        {
            new(1) { Description = "Groceries", BudgetAmount = 500m, BudgetCurrency = CurrencyType.UAH }
        };
        var expected = estimations.ToResponses();

        _repositoryMock.GetAllAsync(Arg.Any<BudgetEstimationsByUserSpecification>()).Returns(estimations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<BudgetEstimationsByUserSpecification>());
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoEstimations()
    {
        // Arrange
        var query = new GetAllBudgetEstimationsQuery();
        _repositoryMock.GetAllAsync(Arg.Any<BudgetEstimationsByUserSpecification>()).Returns(new List<BudgetEstimation>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(Enumerable.Empty<BudgetEstimationResponse>());
    }
}
