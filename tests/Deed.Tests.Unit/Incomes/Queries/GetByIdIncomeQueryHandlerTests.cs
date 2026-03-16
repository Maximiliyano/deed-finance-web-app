using Deed.Application.Auth;
using Deed.Application.Incomes;
using Deed.Application.Incomes.Queries.GetById;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Queries;

public sealed class GetByIdIncomeQueryHandlerTests
{
    private readonly IIncomeRepository _incomeRepositoryMock = Substitute.For<IIncomeRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetIncomeByIdQueryHandler _handler;

    public GetByIdIncomeQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetIncomeByIdQueryHandler(_incomeRepositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ShouldGetIncomeById_WhenExists_ReturnsIncome()
    {
        // Arrange
        var query = new GetIncomeByIdQuery(1);
        var income = new Income(query.Id)
        {
            Amount = 0,
            PaymentDate = default,
            CategoryId = 0,
            CapitalId = 0
        };
        var response = income.ToResponse();

        _incomeRepositoryMock.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns(income);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);

        await _incomeRepositoryMock.Received(1).GetAsync(Arg.Any<IncomeByIdSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldGetIncomeById_WhenNotFound_ReturnNotFound()
    {
        // Arrange
        var query = new GetIncomeByIdQuery(1);

        _incomeRepositoryMock.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns((Income)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("income"));

        await _incomeRepositoryMock.Received(1).GetAsync(Arg.Any<IncomeByIdSpecification>());
    }
}
