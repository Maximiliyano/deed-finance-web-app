using Deed.Application.Expenses.Commands.Update;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class UpdateExpenseCommandHandlerTests
{
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly UpdateExpenseCommandHandler _handler;

    public UpdateExpenseCommandHandlerTests()
    {
        _handler = new UpdateExpenseCommandHandler(_capitalRepository, _categoryRepository, _expenseRepository, _unitOfWork);
    }

    // TODO add test failure createExpenseWithCapitalForSavingsOnly
    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(5, 4, null, null)]
    [InlineData(2, 3, 100.0, null)]
    [InlineData(5, 3, null, "Hi")]
    [InlineData(null, null, 200.0, null)]
    [InlineData(7, null, 200.0, "Well")]
    [InlineData(2, 1, 200.0, null)]
    [InlineData(null, null, null, "Purspo")]
    [InlineData(null, null, 12.0, "Purspo")]
    [InlineData(2, 4, null, "Purspo")]
    [InlineData(8, 1, 150.0, "Test")]
    public async Task Handle_UpdateExpense_ShouldReturnUpdated(
        int? capitalId,
        int? categoryId,
        double? amount,
        string? purpose)
    {
        // Arrange
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
        var command = new UpdateExpenseCommand(id, categoryId, capitalId, newAmount, purpose, utcNow);

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

        if (amount.HasValue || capitalId.HasValue)
        {
            _capitalRepository.Received(1).Update(expense.Capital);
        }
        else
        {
            _capitalRepository.DidNotReceive().Update(expense.Capital);
        }

        if (categoryId.HasValue)
        {
            _categoryRepository.Received(1).Update(expense.Category);
        }
        else
        {
            _categoryRepository.DidNotReceive().Update(expense.Category);
        }

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_UpdateExpense_ShouldReturnFailureWhenNotFound()
    {
        // Arrange
        var command = new UpdateExpenseCommand(1);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns((Expense)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.General.NotFound("expense"));

        _expenseRepository.DidNotReceive().Update(Arg.Any<Expense>());
        _capitalRepository.DidNotReceive().Update(Arg.Any<Capital>());
        _categoryRepository.DidNotReceive().Update(Arg.Any<Category>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
