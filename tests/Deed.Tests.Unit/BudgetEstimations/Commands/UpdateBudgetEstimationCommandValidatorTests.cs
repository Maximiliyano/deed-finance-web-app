using Deed.Application.BudgetEstimations.Commands.Update;
using Deed.Domain.Enums;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BudgetEstimations.Commands;

public sealed class UpdateBudgetEstimationCommandValidatorTests
{
    private readonly UpdateBudgetEstimationCommandValidator _validator = new();

    private static UpdateBudgetEstimationCommand ValidCommand => new(
        1, "Rent", 500m, CurrencyType.UAH, null);

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
    public async Task Validate_NegativeBudgetAmount_FailsValidation()
    {
        var command = ValidCommand with { BudgetAmount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.BudgetAmount);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { BudgetCurrency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.BudgetCurrency);
    }
}
