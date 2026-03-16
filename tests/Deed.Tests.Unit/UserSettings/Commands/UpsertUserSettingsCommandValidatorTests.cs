using Deed.Application.UserSettings.Commands.Upsert;
using Deed.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.UserSettings.Commands;

public sealed class UpsertUserSettingsCommandValidatorTests
{
    private readonly UpsertUserSettingsCommandValidator _validator = new();

    private static UpsertUserSettingsCommand ValidCommand => new(
        5000m, CurrencyType.UAH, false, null, false, null, false, null, false);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NegativeSalary_FailsValidation()
    {
        var command = ValidCommand with { Salary = -1m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Salary);
    }

    [Fact]
    public async Task Validate_ZeroSalary_PassesValidation()
    {
        var command = ValidCommand with { Salary = 0m };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Salary);
    }

    [Fact]
    public async Task Validate_CurrencyNone_FailsValidation()
    {
        var command = ValidCommand with { Currency = CurrencyType.None };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Currency);
    }

    [Fact]
    public async Task Validate_BalanceReminderCronExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { BalanceReminderCron = new string('a', 129) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.BalanceReminderCron);
    }

    [Fact]
    public async Task Validate_BalanceReminderCronAtMaxLength_PassesValidation()
    {
        var command = ValidCommand with { BalanceReminderCron = new string('a', 128) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.BalanceReminderCron);
    }

    [Fact]
    public async Task Validate_ExpenseReminderCronExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { ExpenseReminderCron = new string('a', 129) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.ExpenseReminderCron);
    }

    [Fact]
    public async Task Validate_ExpenseReminderCronAtMaxLength_PassesValidation()
    {
        var command = ValidCommand with { ExpenseReminderCron = new string('a', 128) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.ExpenseReminderCron);
    }

    [Fact]
    public async Task Validate_DebtReminderCronExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand with { DebtReminderCron = new string('a', 129) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.DebtReminderCron);
    }

    [Fact]
    public async Task Validate_DebtReminderCronAtMaxLength_PassesValidation()
    {
        var command = ValidCommand with { DebtReminderCron = new string('a', 128) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(c => c.DebtReminderCron);
    }
}
