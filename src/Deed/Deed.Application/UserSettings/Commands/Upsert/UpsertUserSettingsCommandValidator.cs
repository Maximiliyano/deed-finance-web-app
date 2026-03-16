using Deed.Domain.Constants;
using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.UserSettings.Commands.Upsert;

internal sealed class UpsertUserSettingsCommandValidator : AbstractValidator<UpsertUserSettingsCommand>
{
    public UpsertUserSettingsCommandValidator()
    {
        RuleFor(c => c.Salary)
            .GreaterThanOrEqualTo(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => c != CurrencyType.None)
            .WithMessage("Currency must be set.");

        RuleFor(c => c.BalanceReminderCron)
            .MaximumLength(128);

        RuleFor(c => c.ExpenseReminderCron)
            .MaximumLength(128);

        RuleFor(c => c.DebtReminderCron)
            .MaximumLength(128);
    }
}
