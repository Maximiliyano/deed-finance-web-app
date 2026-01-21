using Deed.Application.Auth;
using Deed.Application.Expenses.Queries.GetAll;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Queries;

public sealed class GetAllExpensesQueryHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();

    private readonly GetExpensesByCategoryHandler _handler;

    public GetAllExpensesQueryHandlerTests()
    {
        _handler = new GetExpensesByCategoryHandler(_userMock, _expenseRepositoryMock);
    }

    [Fact]
    public async Task Handle_GetAllExpenses_ShouldReturnAll()
    {
        // Arrange
        var entity = new Expense
        {
            Amount = 1,
            PaymentDate = DateTimeOffset.UtcNow,
            CategoryId = 1,
            Category = new Category(1)
            {
                Name = "TestCategory",
                Type = CategoryType.Expenses
            },
            CapitalId = 1,
            Capital = new Capital(1)
            {
                Name = "TestCapital",
                Balance = 0,
                Currency = CurrencyType.UAH
            }
        };
        var entities = new List<Expense> { entity };
        var query = new GetExpensesByCategoryQuery(entity.CategoryId);

        _expenseRepositoryMock.GetAllAsync(Arg.Any<ExpenseByQueriesSpecification>()).Returns(entities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().OnlyContain(x => x.CategoryId == query.CategoryId);

        await _expenseRepositoryMock.Received(1).GetAllAsync(Arg.Any<ExpenseByQueriesSpecification>());
    }
}
