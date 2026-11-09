using Deed.Application.Auth;
using Deed.Application.Capitals.Queries.GetAll;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Queries;

public sealed class GetAllCapitalQueryHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetAllCapitalsQueryHandler _handler;

    public GetAllCapitalQueryHandlerTests()
    {
        _handler = new GetAllCapitalsQueryHandler(_userMock, _repositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldMapTotalsFromSummaries()
    {
        // Arrange
        var query = new GetAllCapitalsQuery("TestCapital");
        var capital = new Capital(1) { Name = "TestCapital", Balance = 10, Currency = CurrencyType.USD };

        _repositoryMock
            .GetSummariesAsync(Arg.Any<CapitalsByQueryParamsSpecification>())
            .Returns([new CapitalSummary(capital, 1m, 2m, 3m, 4m)]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var response = result.Value.Should().ContainSingle().Subject;
        response.Id.Should().Be(1);
        response.Name.Should().Be("TestCapital");
        response.Currency.Should().Be("USD");
        response.TotalIncome.Should().Be(1m);
        response.TotalExpense.Should().Be(2m);
        response.TotalTransferIn.Should().Be(3m);
        response.TotalTransferOut.Should().Be(4m);

        await _repositoryMock.Received(1).GetSummariesAsync(Arg.Any<CapitalsByQueryParamsSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCapitals()
    {
        // Arrange
        _repositoryMock
            .GetSummariesAsync(Arg.Any<ISpecification<Capital>>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(new GetAllCapitalsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SortKeysConstants.Expenses, "asc", new[] { 2, 3, 1 })]
    [InlineData(SortKeysConstants.Expenses, "desc", new[] { 1, 3, 2 })]
    [InlineData(SortKeysConstants.Expenses, null, new[] { 1, 3, 2 })]
    [InlineData(SortKeysConstants.Incomes, "asc", new[] { 1, 2, 3 })]
    [InlineData(SortKeysConstants.TransfersIn, "desc", new[] { 3, 2, 1 })]
    [InlineData(SortKeysConstants.TransfersOut, "asc", new[] { 3, 1, 2 })]
    public async Task Handle_ShouldSortByTotals_InMemory(string sortBy, string? sortDirection, int[] expectedIds)
    {
        // Arrange
        var summaries = new List<CapitalSummary>
        {
            Summary(1, income: 10m, expense: 300m, transferIn: 1m, transferOut: 5m),
            Summary(2, income: 20m, expense: 100m, transferIn: 2m, transferOut: 9m),
            Summary(3, income: 30m, expense: 200m, transferIn: 3m, transferOut: 0m)
        };

        _repositoryMock.GetSummariesAsync(Arg.Any<CapitalsByQueryParamsSpecification>()).Returns(summaries);

        // Act
        var result = await _handler.Handle(new GetAllCapitalsQuery(null, sortBy, sortDirection), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Select(c => c.Id).Should().Equal(expectedIds);
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("balance", "desc")]
    [InlineData(null, null)]
    public async Task Handle_ShouldKeepDatabaseOrder_ForNonTotalSortKeys(string? sortBy, string? sortDirection)
    {
        // Arrange
        var summaries = new List<CapitalSummary>
        {
            Summary(2, expense: 1m),
            Summary(1, expense: 9m)
        };

        _repositoryMock.GetSummariesAsync(Arg.Any<CapitalsByQueryParamsSpecification>()).Returns(summaries);

        // Act
        var result = await _handler.Handle(new GetAllCapitalsQuery(null, sortBy, sortDirection), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Select(c => c.Id).Should().Equal(2, 1);
    }

    private static CapitalSummary Summary(int id, decimal income = 0m, decimal expense = 0m, decimal transferIn = 0m, decimal transferOut = 0m) =>
        new(new Capital(id) { Name = $"Capital {id}", Balance = 100m, Currency = CurrencyType.UAH }, income, expense, transferIn, transferOut);
}
