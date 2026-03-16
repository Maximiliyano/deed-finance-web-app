using Deed.Application.Auth;
using Deed.Application.Expenses;
using Deed.Application.Expenses.Queries.GetById;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Queries;

public sealed class GetExpenseByIdQueryHandlerTests
{
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetExpenseByIdQueryHandler _handler;

    public GetExpenseByIdQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetExpenseByIdQueryHandler(_expenseRepositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ShouldGetExpenseById_WhenExists_ReturnExpense()
    {
        // Arrange
        var query = new GetExpenseByIdQuery(1);
        var expense = new Expense(query.Id)
        {
            Amount = 0,
            PaymentDate = default,
            CategoryId = 0,
            CapitalId = 0
        };
        var response = expense.ToResponse();

        _expenseRepositoryMock.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);

        await _expenseRepositoryMock.Received(1).GetAsync(Arg.Any<ExpenseByIdSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldGetExpenseById_WhenNotFound_ReturnNotFound()
    {
        // Arrange
        var query = new GetExpenseByIdQuery(1);

        _expenseRepositoryMock.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns((Expense)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("expense"));

        await _expenseRepositoryMock.Received(1).GetAsync(Arg.Any<ExpenseByIdSpecification>());
    }
}
