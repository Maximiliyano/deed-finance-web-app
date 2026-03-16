using Deed.Application.Goals.Commands.Update;
using Deed.Domain.Enums;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.Goals.Commands;

public sealed class UpdateGoalCommandValidatorTests
{
    private readonly UpdateGoalCommandValidator _validator = new();

    private static UpdateGoalCommand ValidCommand => new(
        1, "Save for car", 10000m, CurrencyType.UAH, 0m, null, null, false);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyTitle_FailsValidation(string? title)
    {
        var command = ValidCommand with { Title = title! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public async Task Validate_TitleExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Title = new string('a', 65) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_TargetAmountNotGreaterThanZero_FailsValidation(decimal amount)
    {
        var command = ValidCommand with { TargetAmount = amount };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.TargetAmount);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Fact]
    public async Task Validate_NegativeCurrentAmount_FailsValidation()
    {
        var command = ValidCommand with { CurrentAmount = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.CurrentAmount);
    }

    [Fact]
    public async Task Validate_NoteExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Note = new string('a', 513) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Note);
    }
}
