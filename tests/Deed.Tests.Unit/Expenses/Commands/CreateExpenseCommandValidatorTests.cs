using Deed.Application.Expenses.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Expenses.Commands;

public sealed class CreateExpenseCommandValidatorTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IDateTimeProvider _provider = Substitute.For<IDateTimeProvider>();
    private readonly CreateExpenseCommandValidator _validator;

    public CreateExpenseCommandValidatorTests()
    {
        _capitalRepository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(true);
        _categoryRepository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(true);
        _categoryRepository.GetAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new Category { Name = "Food", Type = CategoryType.Expenses });
        _provider.UtcNow.Returns(DateTimeOffset.UtcNow);
        _provider.MinValue.Returns(DateTime.MinValue);
        _validator = new CreateExpenseCommandValidator(_categoryRepository, _capitalRepository, _provider);
    }

    private static CreateExpenseCommand ValidCommand => new(
        1, 1, 100m, DateTimeOffset.UtcNow.AddMinutes(-1), null, []);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_CapitalNotFound_FailsValidation()
    {
        _capitalRepository.AnyAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(false);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CapitalId);
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
            .Returns(new Category { Name = "Salary", Type = CategoryType.Incomes });
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
    public async Task Validate_EmptyPurpose_FailsValidation()
    {
        var command = ValidCommand with { Purpose = "" };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Purpose);
    }

    [Fact]
    public async Task Validate_WhitespacePurpose_FailsValidation()
    {
        var command = ValidCommand with { Purpose = "   " };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Purpose);
    }

    [Fact]
    public async Task Validate_NullPurpose_PassesValidation()
    {
        var command = ValidCommand with { Purpose = null };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Purpose);
    }
}
