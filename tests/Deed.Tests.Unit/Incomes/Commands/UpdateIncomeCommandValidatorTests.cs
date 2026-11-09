using Deed.Application.Incomes.Commands.Update;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Commands;

public sealed class UpdateIncomeCommandValidatorTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly UpdateIncomeCommandValidator _validator;

    public UpdateIncomeCommandValidatorTests()
    {
        _categoryRepository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(true);
        _validator = new UpdateIncomeCommandValidator(_categoryRepository);
    }

    [Fact]
    public async Task Validate_IncomeCategory_PassesValidation()
    {
        // Arrange
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Salary", Type = CategoryType.Incomes });

        // Act
        var result = await _validator.TestValidateAsync(new UpdateIncomeCommand(1, CategoryId: 2));

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_ExpenseCategory_FailsValidation()
    {
        // Arrange
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Food", Type = CategoryType.Expenses });

        // Act
        var result = await _validator.TestValidateAsync(new UpdateIncomeCommand(1, CategoryId: 2));

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_NoCategoryProvided_PassesValidation()
    {
        // Act
        var result = await _validator.TestValidateAsync(new UpdateIncomeCommand(1));

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
