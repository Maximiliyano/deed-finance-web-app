using System.Linq;
using System.Collections.Generic;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
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
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateExpenseCommandHandler _handler;

    public UpdateExpenseCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Tag>());
        _handler = new UpdateExpenseCommandHandler(_expenseRepository, _capitalRepository, _tagRepository, _unitOfWork, _userMock);
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

        var capitalChanged = capitalId.HasValue && capitalId.Value != oldCapitalId;
        var finalAmount = newAmount ?? oldAmount;

        Capital? target = null;
        if (capitalChanged)
        {
            target = new Capital(capitalId!.Value) { Name = "Target", Balance = 1000, Currency = CurrencyType.UAH };
            _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(target);
        }

        var expectedOldCapitalBalance = capitalChanged
            ? 1000m + oldAmount
            : 1000m + oldAmount - finalAmount;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        expense.Id.Should().Be(id);
        expense.Amount.Should().Be(newAmount ?? oldAmount);
        expense.Capital.Balance.Should().Be(expectedOldCapitalBalance);
        if (target is not null)
        {
            target.Balance.Should().Be(1000m - finalAmount);
            _capitalRepository.Received(1).Update(target);
        }

        expense.Purpose.Should().Be(purpose);
        expense.PaymentDate.Should().Be(utcNow);
        expense.CategoryId.Should().Be(categoryId ?? oldCategoryId);
        expense.CapitalId.Should().Be(capitalId ?? oldCapitalId);

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPurposeIsNull_ClearsPurpose()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        expense.Purpose = "Groceries";

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);

        // Act
        var result = await _handler.Handle(new UpdateExpenseCommand(expense.Id, Purpose: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense.Purpose.Should().BeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagNamesIsNull_KeepsExistingTags()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        expense.Tags.Add(new ExpenseTag { Expense = expense, Tag = new Tag { Id = 5, Name = "food" }, TagId = 5 });

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);

        // Act
        await _handler.Handle(new UpdateExpenseCommand(expense.Id, TagNames: null), CancellationToken.None);

        // Assert
        expense.Tags.Should().ContainSingle().Which.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenTagNamesIsEmpty_RemovesAllTags()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        expense.Tags.Add(new ExpenseTag { Expense = expense, Tag = new Tag { Id = 5, Name = "food" }, TagId = 5 });

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);

        // Act
        var result = await _handler.Handle(new UpdateExpenseCommand(expense.Id, Purpose: expense.Purpose, TagNames: []), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense.Tags.Should().ContainSingle().Which.IsDeleted.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagAlreadyExistsInDatabase_ReusesItInsteadOfCreatingDuplicate()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        var existingTag = new Tag { Id = 7, Name = "Food" };

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { existingTag });

        // Act
        await _handler.Handle(new UpdateExpenseCommand(expense.Id, Purpose: expense.Purpose, TagNames: ["food"]), CancellationToken.None);

        // Assert
        expense.Tags.Should().ContainSingle().Which.Tag.Should().BeSameAs(existingTag);
    }

    [Fact]
    public async Task Handle_WhenReAddingRemovedTag_RevivesSoftDeletedLink()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        var tag = new Tag { Id = 7, Name = "food" };
        var deletedLink = new ExpenseTag { Expense = expense, Tag = tag, TagId = 7, IsDeleted = true };
        expense.Tags.Add(deletedLink);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { tag });

        // Act
        await _handler.Handle(new UpdateExpenseCommand(expense.Id, Purpose: expense.Purpose, TagNames: ["food"]), CancellationToken.None);

        // Assert
        expense.Tags.Should().ContainSingle().Which.Should().BeSameAs(deletedLink);
        deletedLink.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenMovingToAnotherCapital_RefundsOldAndChargesNew()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        var target = new Capital(9) { Name = "Target", Balance = 500m, Currency = CurrencyType.UAH };

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(target);

        var command = new UpdateExpenseCommand(expense.Id, CapitalId: 9);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense.Capital!.Balance.Should().Be(1100m);
        target.Balance.Should().Be(400m);
        expense.CapitalId.Should().Be(9);

        _capitalRepository.Received(1).Update(target);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTargetCapitalNotOwned_ReturnsNotFoundAndDoesNotChangeBalances()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns((Capital?)null);

        var command = new UpdateExpenseCommand(expense.Id, CapitalId: 9);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("capital"));
        expense.Capital!.Balance.Should().Be(1000m);
        expense.CapitalId.Should().Be(1);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTargetCapitalIsSavingsOnly_ReturnsFailure()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        var target = new Capital(9) { Name = "Savings", Balance = 500m, Currency = CurrencyType.UAH, OnlyForSavings = true };

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _capitalRepository.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(target);

        var command = new UpdateExpenseCommand(expense.Id, CapitalId: 9);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e.Equals(DomainErrors.Capital.ForSavingsOnly));
        target.Balance.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_WhenMovingCapital_LooksUpTargetScopedToCurrentUser()
    {
        // Arrange
        var expense = CreateExpense(amount: 100m, capitalBalance: 1000m);
        ISpecification<Capital>? captured = null;

        _expenseRepository.GetAsync(Arg.Any<ExpenseByIdSpecification>()).Returns(expense);
        _capitalRepository.GetAsync(Arg.Do<ISpecification<Capital>>(s => captured = s), Arg.Any<CancellationToken>())
            .Returns(new Capital(9) { Name = "Target", Balance = 500m, Currency = CurrencyType.UAH });

        // Act
        await _handler.Handle(new UpdateExpenseCommand(expense.Id, CapitalId: 9), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        var matches = captured!.Criteria!.Compile();
        matches(new Capital(9) { Name = "x", Balance = 0, Currency = CurrencyType.UAH, CreatedBy = "testuser" }).Should().BeTrue();
        matches(new Capital(9) { Name = "x", Balance = 0, Currency = CurrencyType.UAH, CreatedBy = "someone-else" }).Should().BeFalse();
    }

    private static Expense CreateExpense(decimal amount, decimal capitalBalance) => new(1)
    {
        Amount = amount,
        PaymentDate = DateTimeOffset.UtcNow,
        CategoryId = 2,
        Category = new Category(2) { Name = "TestCategory", Type = CategoryType.Expenses },
        CapitalId = 1,
        Capital = new Capital(1) { Name = "Old", Balance = capitalBalance, Currency = CurrencyType.UAH }
    };

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
