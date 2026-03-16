using Deed.Application.BudgetEstimations.Commands.Create;
using Deed.Domain.Enums;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BudgetEstimations.Commands;

public sealed class CreateBudgetEstimationCommandValidatorTests
{
    private readonly CreateBudgetEstimationCommandValidator _validator = new();

    private static CreateBudgetEstimationCommand ValidCommand => new(
        "Rent", 500m, CurrencyType.UAH, null);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyDescription_FailsValidation(string? description)
    {
        var command = ValidCommand with { Description = description! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public async Task Validate_DescriptionExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Description = new string('a', 65) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public async Task Validate_DescriptionAtMaxLength_PassesValidation()
    {
        var command = ValidCommand with { Description = new string('a', 64) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public async Task Validate_NegativeBudgetAmount_FailsValidation()
    {
        var command = ValidCommand with { BudgetAmount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.BudgetAmount);
    }

    [Fact]
    public async Task Validate_ZeroBudgetAmount_PassesValidation()
    {
        var command = ValidCommand with { BudgetAmount = 0m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.BudgetAmount);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { BudgetCurrency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.BudgetCurrency);
    }
}
