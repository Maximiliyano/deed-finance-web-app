using System.Xml.Linq;
using Deed.Application.Capitals;
using Deed.Application.Capitals.Queries.GetAll;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Queries;

public sealed class GetAllCapitalQueryHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();

    private readonly GetAllCapitalsQueryHandler _handler;

    public GetAllCapitalQueryHandlerTests()
    {
        _handler = new GetAllCapitalsQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldGetAllExistingCapitals_ReturnsList()
    {
        // Arrange
        const int capitalId = 1;

        var capitalName = "TestCapital";
        var query = new GetAllCapitalsQuery(capitalName);
        var capitals = new List<Capital> { new(capitalId)
            {
                Name = capitalName,
                Balance = 10,
                Currency = CurrencyType.USD
            }
        };
        var capitalResponses = capitals
            .Select(x => new CapitalResponse(
                x.Id,
                x.Name,
                x.Balance,
                x.Currency.ToString(),
                x.IncludeInTotal,
                x.OnlyForSavings,
                x.TotalIncome,
                x.TotalExpense,
                x.TotalTransferIn,
                x.TotalTransferOut,
                x.CreatedAt,
                x.CreatedBy
            ));

        _repositoryMock.GetAllAsync(Arg.Any<CapitalsByQueryParamsSpecification>()).Returns(capitals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(capitalResponses);

            await _repositoryMock.Received(1).GetAllAsync(Arg.Any<CapitalsByQueryParamsSpecification>());
        }
    }

    [Fact]
    public async Task Handle_ShouldGetAllCapitals_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllCapitalsQuery();
        var capitals = new List<Capital>();
        var capitalResponses = capitals.Select(_ => new CapitalResponse(
            0,
            string.Empty,
            0m,
            string.Empty,
            false,
            false,
            0m,
            0m,
            0m,
            0m,
            DateTimeOffset.Now,
            string.Empty));

        _repositoryMock.GetAllAsync(Arg.Any<ISpecification<Capital>>()).Returns(capitals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(capitalResponses);

            await _repositoryMock.Received(1).GetAllAsync(Arg.Any<ISpecification<Capital>>());
        }
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("balance", "asc")]
    [InlineData("expenses", "desc")]
    [InlineData(null, null)]
    public async Task Handle_ShouldGetAllCapitalsBySortParams_ReturnsSortedCapitals(string? sortBy, string? sortDirection)
    {
        // Arrange
        var query = new GetAllCapitalsQuery(null, sortBy, sortDirection);
        var capitals = new List<Capital>
        {
            new(1) { Name = "Berlin", Balance = 100, Currency = CurrencyType.USD, OrderIndex = 2 },
            new(2) { Name = "Paris", Balance = 200, Currency = CurrencyType.UAH, OrderIndex = 1 }
        };
        var response = capitals.ToResponses();

        _repositoryMock
            .GetAllAsync(Arg.Any<CapitalsByQueryParamsSpecification>())
            .Returns(capitals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);

        await _repositoryMock.Received(1)
            .GetAllAsync(Arg.Any<CapitalsByQueryParamsSpecification>());
    }
}
