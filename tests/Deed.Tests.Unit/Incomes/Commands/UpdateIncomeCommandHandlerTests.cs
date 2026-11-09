using Deed.Application.Auth;
using Deed.Application.Incomes.Commands.Update;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Commands;

public sealed class UpdateIncomeCommandHandlerTests
{
    private readonly IIncomeRepository _incomeRepository = Substitute.For<IIncomeRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateIncomeCommandHandler _handler;

    public UpdateIncomeCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Tag>());
        _handler = new UpdateIncomeCommandHandler(_incomeRepository, _tagRepository, _unitOfWork, _userMock);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(4, null, null)]
    [InlineData(3, 100.0, null)]
    [InlineData(3, null, "Hi")]
    [InlineData(null, 200.0, null)]
    [InlineData(null, 200.0, "Well")]
    [InlineData(1, 200.0, null)]
    [InlineData(null, null, "Purspo")]
    [InlineData(null, 12.0, "Purspo")]
    [InlineData(4, null, "Purspo")]
    [InlineData(1, 150.0, "Test")]
    public async Task Handle_ShouldReturnSuccess_WhenIncomeUpdatedSuccessfully(
        int? categoryId,
        double? amount,
        string? purpose)
    {
        // Arrange
        const int id = 1;
        const int oldCategoryId = 2;
        const decimal oldAmount = 100m;
        const string oldPurpose = "Hello";

        var utcNow = DateTimeOffset.UtcNow;
        var income = new Income(id)
        {
            Amount = oldAmount,
            PaymentDate = utcNow.AddDays(2),
            CategoryId = oldCategoryId,
            Category = new Category(oldCategoryId)
            {
                Name = "TestCategory",
                Type = CategoryType.Incomes
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
        decimal? newAmount = amount is null ? null : (decimal)amount;
        var command = new UpdateIncomeCommand(id, categoryId, newAmount, purpose, utcNow);

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns(income);

        var expectedCapitalBalance = newAmount.HasValue
            ? income.Capital.Balance + newAmount.Value - income.Amount
            : income.Capital.Balance;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        income.Id.Should().Be(id);
        income.Amount.Should().Be(newAmount ?? oldAmount);
        income.Capital.Balance.Should().Be(expectedCapitalBalance);
        income.Purpose.Should().Be(purpose);
        income.PaymentDate.Should().Be(utcNow);
        income.CategoryId.Should().Be(categoryId ?? oldCategoryId);

        _incomeRepository.DidNotReceive().Update(Arg.Any<Income>());

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenTagNamesIsEmpty_RemovesAllTags()
    {
        // Arrange
        var income = CreateIncome();
        income.Tags.Add(new IncomeTag { Income = income, Tag = new Tag { Id = 5, Name = "bonus" }, TagId = 5 });

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>()).Returns(income);

        // Act
        var result = await _handler.Handle(new UpdateIncomeCommand(1, Purpose: income.Purpose, TagNames: []), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        income.Tags.Should().ContainSingle().Which.IsDeleted.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagAlreadyExistsInDatabase_ReusesItInsteadOfCreatingDuplicate()
    {
        // Arrange
        var income = CreateIncome();
        var existingTag = new Tag { Id = 9, Name = "Bonus" };

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>()).Returns(income);
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { existingTag });

        // Act
        await _handler.Handle(new UpdateIncomeCommand(1, Purpose: income.Purpose, TagNames: ["bonus"]), CancellationToken.None);

        // Assert
        income.Tags.Should().ContainSingle().Which.Tag.Should().BeSameAs(existingTag);
    }

    [Fact]
    public async Task Handle_WhenReAddingRemovedTag_RevivesSoftDeletedLink()
    {
        // Arrange
        var income = CreateIncome();
        var tag = new Tag { Id = 9, Name = "bonus" };
        var deletedLink = new IncomeTag { Income = income, Tag = tag, TagId = 9, IsDeleted = true };
        income.Tags.Add(deletedLink);

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>()).Returns(income);
        _tagRepository.GetAllAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { tag });

        // Act
        await _handler.Handle(new UpdateIncomeCommand(1, Purpose: income.Purpose, TagNames: ["bonus"]), CancellationToken.None);

        // Assert
        income.Tags.Should().ContainSingle().Which.Should().BeSameAs(deletedLink);
        deletedLink.IsDeleted.Should().BeFalse();
    }

    private static Income CreateIncome() => new(1)
    {
        Amount = 100m,
        PaymentDate = DateTimeOffset.UtcNow,
        CategoryId = 2,
        Category = new Category(2) { Name = "TestCategory", Type = CategoryType.Incomes },
        CapitalId = 1,
        Capital = new Capital(1) { Name = "TestCapital", Balance = 1000, Currency = CurrencyType.UAH },
        Purpose = "Hello"
    };

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIncomeNotFound()
    {
        // Arrange
        var command = new UpdateIncomeCommand(1);

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns((Income)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(x => x == DomainErrors.General.NotFound("income"));

        _incomeRepository.DidNotReceive().Update(Arg.Any<Income>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
