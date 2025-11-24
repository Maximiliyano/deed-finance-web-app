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
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly UpdateIncomeCommandHandler _handler;

    public UpdateIncomeCommandHandlerTests()
    {
        _handler = new UpdateIncomeCommandHandler(_incomeRepository, _capitalRepository, _categoryRepository, _unitOfWork);
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
        income.Purpose.Should().Be(purpose ?? oldPurpose);
        income.PaymentDate.Should().Be(utcNow);
        income.CategoryId.Should().Be(categoryId ?? oldCategoryId);

        _incomeRepository.Received(1).Update(income);

        if (amount.HasValue)
        {
            _capitalRepository.Received(1).Update(income.Capital);
        }
        else
        {
            _capitalRepository.DidNotReceive().Update(income.Capital);
        }

        if (categoryId.HasValue)
        {
            _categoryRepository.Received(1).Update(income.Category);
        }
        else
        {
            _categoryRepository.DidNotReceive().Update(income.Category);
        }

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

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
        _capitalRepository.DidNotReceive().Update(Arg.Any<Capital>());
        _categoryRepository.DidNotReceive().Update(Arg.Any<Category>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
