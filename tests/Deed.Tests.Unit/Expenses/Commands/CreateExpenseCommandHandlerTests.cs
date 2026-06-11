using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Expenses.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class CreateExpenseCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly ITagRepository _tagRepositoryMock = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateExpenseCommandHandler _handler;

    public CreateExpenseCommandHandlerTests()
    {
        _userMock.IsAuthenticated.Returns(true);
        _handler = new CreateExpenseCommandHandler(_userMock, _capitalRepositoryMock, _expenseRepositoryMock, _tagRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateExpenseWithCapitalForSavingsOnly_ShouldReturnsFailure()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 100,
            Currency = CurrencyType.UAH,
            OnlyForSavings = true
        };
        var category = new Category(1)
        {
            Name = "TestCategory",
            Type = CategoryType.Expenses
        };
        var command = new CreateExpenseCommand(capital.Id, category.Id, 10m, DateTimeOffset.UtcNow, null, []);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
            .Returns(capital);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Capital.ForSavingsOnly);

        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        
        await _capitalRepositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_CreateExpense_ShouldReturnsId()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 100,
            Currency = CurrencyType.UAH
        };
        var category = new Category(1)
        {
            Name = "TestCategory",
            Type = CategoryType.Expenses
        };

        var command = new CreateExpenseCommand(capital.Id, category.Id, 10m, DateTimeOffset.UtcNow, null, []);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
            .Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        capital.Balance.Should().Be(90m);

        _capitalRepositoryMock.Received(1).Update(capital);
        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e => e.Amount.Equals(command.Amount)));

        await _capitalRepositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_CreateExpenseWhenCapitalNotFound_ShouldReturnsFailure()
    {
        // Arrange
        var command = new CreateExpenseCommand(1, 1, 10m, DateTimeOffset.UtcNow, null, []);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
            .Returns((Capital)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("capital"));

        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());

        await _capitalRepositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreateTags_WhenTagNamesProvidedAndNoneExist()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 100,
            Currency = CurrencyType.UAH
        };

        var command = new CreateExpenseCommand(capital.Id, 1, 10m, DateTimeOffset.UtcNow, null, ["food", "fun"]);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _tagRepositoryMock.GetAllAsync(Arg.Any<ISpecification<Tag>>()).Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepositoryMock.Received(2).Create(Arg.Any<Tag>());
    }

    [Fact]
    public async Task Handle_ShouldLinkExistingTagAndCreateOnlyNewOnes_WhenTagAlreadyExists()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 100,
            Currency = CurrencyType.UAH
        };

        var existingTag = new Tag { Name = "food" };

        var command = new CreateExpenseCommand(capital.Id, 1, 10m, DateTimeOffset.UtcNow, null, ["food", "fun"]);

        Expense? createdExpense = null;

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _tagRepositoryMock.GetAllAsync(Arg.Any<ISpecification<Tag>>()).Returns([existingTag]);
        _expenseRepositoryMock.When(r => r.Create(Arg.Any<Expense>())).Do(ci => createdExpense = ci.Arg<Expense>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tagRepositoryMock.Received(1).Create(Arg.Any<Tag>());

        createdExpense.Should().NotBeNull();
        createdExpense!.Tags.Should().ContainSingle(t => t.Tag == existingTag);
    }
}
