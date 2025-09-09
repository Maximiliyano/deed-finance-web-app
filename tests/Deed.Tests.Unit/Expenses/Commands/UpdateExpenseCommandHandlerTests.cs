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
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly UpdateExpenseCommandHandler _handler;

    public UpdateExpenseCommandHandlerTests()
    {
        _handler = new UpdateExpenseCommandHandler(_dateTimeProvider, _capitalRepository, _categoryRepository, _expenseRepository, _unitOfWork);
    }

    // TODO add test failure createExpenseWithCapitalForSavingsOnly
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(4, null, null)]
    [InlineData(3, 100f, null)]
    [InlineData(3, null, "Hi")]
    [InlineData(null, 200f, null)]
    [InlineData(null, 200f, "Well")]
    [InlineData(1, 200f, null)]
    [InlineData(null, null, "Purspo")]
    [InlineData(null, 12f, "Purspo")]
    [InlineData(4, null, "Purspo")]
    [InlineData(1, 150f, "Test")]
    public async Task Handle_UpdateExpense_ShouldReturnUpdated(
        int? categoryId,
        float? amount,
        string? purpose)
    {
        // Arrange
        const int id = 1;
        const int oldCategoryId = 2;
        const float oldAmount = 100f;
        const string oldPurpose = "Hello";

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
            CapitalId = 1,
            Capital = new Capital(1)
            {
                Name = "TestCapital",
                Balance = 1000,
                Currency = CurrencyType.UAH
            },
            Purpose = oldPurpose
        };
        var command = new UpdateExpenseCommand(id, categoryId, amount, purpose, utcNow);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>())
            .Returns(expense);

        _dateTimeProvider.UtcNow.Returns(utcNow);

        var expectedCapitalBalance = amount.HasValue
            ? expense.Capital.Balance + expense.Amount - amount.Value
            : expense.Capital.Balance;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        expense.Id.Should().Be(id);
        expense.Amount.Should().Be(amount ?? oldAmount);
        expense.Capital.Balance.Should().Be(expectedCapitalBalance);
        expense.Purpose.Should().Be(purpose ?? oldPurpose);
        expense.PaymentDate.Should().Be(utcNow);
        expense.CategoryId.Should().Be(categoryId ?? oldCategoryId);

        _expenseRepository.Received(1).Update(expense);

        if (amount.HasValue)
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
