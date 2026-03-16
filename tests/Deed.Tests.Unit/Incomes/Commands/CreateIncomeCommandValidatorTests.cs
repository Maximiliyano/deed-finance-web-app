using Deed.Application.Incomes.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Commands;

public sealed class CreateIncomeCommandValidatorTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly CreateIncomeCommandValidator _validator;

    public CreateIncomeCommandValidatorTests()
    {
        _categoryRepository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(true);
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Salary", Type = CategoryType.Incomes });
        _validator = new CreateIncomeCommandValidator(_categoryRepository);
    }

    private static CreateIncomeCommand ValidCommand => new(
        1, 1, 500m, DateTimeOffset.UtcNow, null, new List<string>());

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_CategoryNotFound_FailsValidation()
    {
        _categoryRepository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(false);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_WrongCategoryType_FailsValidation()
    {
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Food", Type = CategoryType.Expenses });
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Validate_NegativeAmount_FailsValidation()
    {
        var command = ValidCommand with { Amount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public async Task Validate_ZeroAmount_PassesValidation()
    {
        var command = ValidCommand with { Amount = 0m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Amount);
    }
}
