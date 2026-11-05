using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using FluentValidation;

namespace Deed.Application.Debts.Commands.Update;

internal sealed class UpdateDebtCommandValidator : AbstractValidator<UpdateDebtCommand>
{
    public UpdateDebtCommandValidator()
    {
        RuleFor(c => c.Item)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Amount)
            .GreaterThan(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => Enum.TryParse<CurrencyType>(c, out var pc) && !pc.Equals(CurrencyType.None))
            .WithError(ValidationErrors.General.InvalidCurrencyType);

        RuleFor(c => c.Source)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Recipient)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(c => c.Note)
            .MaximumLength(512);
    }
}
