using Deed.Application.UtilityBills.Commands.Create;
using Deed.Domain.Enums;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class CreateUtilityBillCommandValidatorTests
{
    private readonly CreateUtilityBillCommandValidator _validator = new();

    private static CreateUtilityBillCommand ValidCommand => new(
        "Electric", 500m, CurrencyType.UAH, 15, 1, 7, true);

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
        var command = ValidCommand with { Name = new string('a', 65) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validate_NegativeEstimatedAmount_FailsValidation()
    {
        var command = ValidCommand with { EstimatedAmount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.EstimatedAmount);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(-1)]
    public async Task Validate_DueDayOutOfRange_FailsValidation(int day)
    {
        var command = ValidCommand with { DueDayOfMonth = day };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.DueDayOfMonth);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(31)]
    public async Task Validate_DueDayInRange_PassesValidation(int day)
    {
        var command = ValidCommand with { DueDayOfMonth = day };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.DueDayOfMonth);
    }

    [Fact]
    public async Task Validate_ZeroCapitalId_FailsValidation()
    {
        var command = ValidCommand with { CapitalId = 0 };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.CapitalId);
    }

    [Fact]
    public async Task Validate_ZeroCategoryId_FailsValidation()
    {
        var command = ValidCommand with { CategoryId = 0 };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }
}
