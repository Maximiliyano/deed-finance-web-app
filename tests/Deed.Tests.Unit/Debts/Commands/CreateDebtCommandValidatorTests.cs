using Deed.Application.Debts.Commands.Create;
using Deed.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class CreateDebtCommandValidatorTests
{
    private readonly CreateDebtCommandValidator _validator = new();

    private static CreateDebtCommand ValidCommand => new(
        "Laptop", 1500m, CurrencyType.USD,
        "John", "Me", DateTimeOffset.UtcNow,
        null, null, null);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyItem_FailsValidation(string? item)
    {
        var command = ValidCommand with { Item = item! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Item);
    }

    [Fact]
    public async Task Validate_ItemExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Item = new string('a', 65) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Item);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_AmountNotGreaterThanZero_FailsValidation(decimal amount)
    {
        var command = ValidCommand with { Amount = amount };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptySource_FailsValidation(string? source)
    {
        var command = ValidCommand with { Source = source! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Source);
    }

    [Fact]
    public async Task Validate_SourceExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Source = new string('a', 129) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Source);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyRecipient_FailsValidation(string? recipient)
    {
        var command = ValidCommand with { Recipient = recipient! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Recipient);
    }

    [Fact]
    public async Task Validate_RecipientExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Recipient = new string('a', 129) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Recipient);
    }

    [Fact]
    public async Task Validate_NoteExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { Note = new string('a', 513) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Note);
    }

    [Fact]
    public async Task Validate_NoteWithinMaxLength_PassesValidation()
    {
        var command = ValidCommand with { Note = new string('a', 512) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Note);
    }
}
