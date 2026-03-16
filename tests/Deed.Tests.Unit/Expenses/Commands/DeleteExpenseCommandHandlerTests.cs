using Deed.Application.Auth;
using Deed.Application.Expenses.Commands.Delete;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class DeleteExpenseCommandHandlerTests
{
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteExpenseCommandHandler _handler;

    public DeleteExpenseCommandHandlerTests()
    {
        _userMock.Name.Returns("tester");
        _handler = new DeleteExpenseCommandHandler(_capitalRepositoryMock, _expenseRepositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_ShouldDeleteExpense_ReturnsNoContent()
    {
        // Arrange
        var expense = new Expense(1)
        {
            Amount = 10,
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
                Balance = 100,
                Currency = CurrencyType.USD
            }
        };

        var command = new DeleteExpenseCommand(expense.Id);

        _expenseRepositoryMock.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        expense.Capital.Balance.Should().Be(110);

        _capitalRepositoryMock.Received(1).Update(expense.Capital);
        _expenseRepositoryMock.Received(1).Delete(expense);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
        await _expenseRepositoryMock.Received(1).GetAsync(Arg.Any<ExpenseByIdSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenExpenseNotFound()
    {
        // Arrange
        var command = new DeleteExpenseCommand(1);

        _expenseRepositoryMock.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns((Expense)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("expense"));

        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        _expenseRepositoryMock.DidNotReceive().Delete(Arg.Any<Expense>());

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync();
        await _expenseRepositoryMock.Received(1).GetAsync(Arg.Any<ExpenseByIdSpecification>());
    }
}
