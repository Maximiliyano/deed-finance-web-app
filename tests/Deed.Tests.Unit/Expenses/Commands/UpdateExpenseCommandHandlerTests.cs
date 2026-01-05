using System.Linq;
using System.Collections.Generic;
using Deed.Application.Expenses.Commands.Update;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class UpdateExpenseCommandHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly UpdateExpenseCommandHandler _handler;

    public UpdateExpenseCommandHandlerTests()
    {
        _handler = new UpdateExpenseCommandHandler(_expenseRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_UpdateExpenseWithCapitialForSavingsOnly_ShouldReturnFailure()
    {
        // Arrange
        var expense = new Expense(1)
        {
            Amount = 10.0m,
            PaymentDate = DateTimeOffset.UtcNow,
            CategoryId = 1,
            Category = new Category(1)
            {
                Name = "ExpenseCategory",
                Type = CategoryType.Expenses
            },
            CapitalId = 1,
            Capital = new Capital(1)
            {
                Name = "SavingsCapital",
                Balance = 500.0m,
                Currency = CurrencyType.USD,
                OnlyForSavings = true
            }
        };

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        var command = new UpdateExpenseCommand(expense.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e.Equals(DomainErrors.Capital.ForSavingsOnly));

        _expenseRepository.DidNotReceive().Update(Arg.Any<Expense>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(CancellationToken.None);
    }

    [Theory]
    [InlineData(null, null, null, null, null)]
    [InlineData(5, 4, null, null, null)]
    [InlineData(2, 3, 100.0, null, null)]
    [InlineData(5, 3, null, "Hi", null)]
    [InlineData(null, null, 200.0, null, null)]
    [InlineData(7, null, 200.0, "Well", null)]
    [InlineData(2, 1, 200.0, null, null)]
    [InlineData(null, null, null, "Purspo", null)]
    [InlineData(null, null, 12.0, "Purspo", new string[] { })]
    [InlineData(2, 4, null, "Purspo", null)]
    [InlineData(8, 1, 150.0, "Test", new string[] { })]
    public async Task Handle_UpdateExpense_ShouldReturnUpdated(
        int? capitalId,
        int? categoryId,
        double? amount,
        string? purpose,
        string[]? tagNamesArray
        )
    {
        // Arrange
        List<string>? tagNames = tagNamesArray?.ToList();

        const int id = 1;
        const int oldCategoryId = 2;
        const int oldCapitalId = 1;
        const decimal oldAmount = 100m;

        var utcNow = DateTimeOffset.UtcNow;
        var expense = new Expense(id)
        {
            Amount = oldAmount,
            PaymentDate = utcNow.AddDays(2),
            CategoryId = oldCategoryId,
            Category = new Category(oldCategoryId)
            {
                Name = "TestCategory",
                Type = CategoryType.Expenses
            },
            CapitalId = oldCapitalId,
            Capital = new Capital(oldCapitalId)
            {
                Name = "TestCapital",
                Balance = 1000,
                Currency = CurrencyType.UAH
            },
            Purpose = purpose
        };
        decimal? newAmount = amount is null ? null : (decimal)amount;
        var command = new UpdateExpenseCommand(id, categoryId, capitalId, newAmount, purpose, tagNames, Date: utcNow);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        var expectedCapitalBalance = newAmount.HasValue
            ? expense.Capital.Balance + expense.Amount - newAmount.Value
            : expense.Capital.Balance;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        expense.Id.Should().Be(id);
        expense.Amount.Should().Be(newAmount ?? oldAmount);
        expense.Capital.Balance.Should().Be(expectedCapitalBalance);
        expense.Purpose.Should().Be(purpose);
        expense.PaymentDate.Should().Be(utcNow);
        expense.CategoryId.Should().Be(categoryId ?? oldCategoryId);
        expense.CapitalId.Should().Be(capitalId ?? oldCapitalId);

        _expenseRepository.Received(1).Update(expense);

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_UpdateExpense_ShouldReturnFailureWhenNotFound()
    {
        // Arrange
        var command = new UpdateExpenseCommand(1);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns((Expense?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.General.NotFound("expense"));

        _expenseRepository.DidNotReceive().Update(Arg.Any<Expense>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_NoChanges_ShouldReturnSuccessAndNotUpdateOrSave()
    {
        // Arrange
        const int id = 42;
        var now = DateTimeOffset.UtcNow;
        var expense = new Expense(id)
        {
            Amount = 10m,
            PaymentDate = now,
            CategoryId = 1,
            Category = new Category(1) { Name = "c", Type = CategoryType.Expenses },
            CapitalId = 2,
            Capital = new Capital(2) { Name = "cap", Balance = 100, Currency = CurrencyType.UAH },
            Purpose = "same"
        };

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        var command = new UpdateExpenseCommand(id, expense.CategoryId, expense.CapitalId, expense.Amount, expense.Purpose, null, Date: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _expenseRepository.DidNotReceive().Update(Arg.Any<Expense>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
