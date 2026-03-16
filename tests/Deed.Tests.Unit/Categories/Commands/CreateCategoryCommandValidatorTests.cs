using Deed.Application.Categories.Commands.Create;
using Deed.Domain.Enums;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Commands;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _repository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(false);
        _validator = new CreateCategoryCommandValidator(_repository);
    }

    private static CreateCategoryCommand ValidCommand => new(
        "Food", CategoryType.Expenses, 0m, PerPeriodType.None);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyName_FailsValidation(string? name)
    {
        var command = ValidCommand with { Name = name! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_NameExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Name = new string('a', 33) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_NameAlreadyExists_FailsValidation()
    {
        _repository.AnyAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>()).Returns(true);
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_CategoryTypeNone_FailsValidation()
    {
        var command = ValidCommand with { Type = CategoryType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public async Task Validate_NegativePlannedAmount_FailsValidation()
    {
        var command = ValidCommand with { PlannedPeriodAmount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.PlannedPeriodAmount);
    }

    [Fact]
    public async Task Validate_ZeroPlannedAmount_PassesValidation()
    {
        var command = ValidCommand with { PlannedPeriodAmount = 0m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.PlannedPeriodAmount);
    }
}
