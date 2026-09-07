using Deed.Application.UtilityBills.Commands.Pay;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class PayUtilityBillCommandValidatorTests
{
    private readonly PayUtilityBillCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = new PayUtilityBillCommand(1, 100m, null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ZeroId_FailsValidation()
    {
        var command = new PayUtilityBillCommand(0, 100m, null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public async Task Validate_ZeroAmount_FailsValidation()
    {
        var command = new PayUtilityBillCommand(1, 0m, null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public async Task Validate_NegativeAmount_FailsValidation()
    {
        var command = new PayUtilityBillCommand(1, -5m, null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }
}
