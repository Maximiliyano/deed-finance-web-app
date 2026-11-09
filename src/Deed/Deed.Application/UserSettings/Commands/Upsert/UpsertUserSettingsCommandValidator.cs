using Deed.Application.Abstractions;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using FluentValidation;

namespace Deed.Application.UserSettings.Commands.Upsert;

internal sealed class UpsertUserSettingsCommandValidator : AbstractValidator<UpsertUserSettingsCommand>
{
    public UpsertUserSettingsCommandValidator()
    {
        RuleFor(c => c.Currency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .WithError(ValidationErrors.General.InvalidCurrencyType);

        RuleFor(c => c.BalanceReminderCron)
            .MaximumLength(128);

        RuleFor(c => c.ExpenseReminderCron)
            .MaximumLength(128);

        RuleFor(c => c.DebtReminderCron)
            .MaximumLength(128);
    }
}
