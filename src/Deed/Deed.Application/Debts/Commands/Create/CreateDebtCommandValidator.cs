using Deed.Domain.Constants;
using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.Debts.Commands.Create;

internal sealed class CreateDebtCommandValidator : AbstractValidator<CreateDebtCommand>
{
    public CreateDebtCommandValidator()
    {
        RuleFor(c => c.Item)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Amount)
            .GreaterThan(ValidationConstants.ZeroValue);

        RuleFor(c => c.Currency)
            .Must(c => c != CurrencyType.None)
            .WithMessage("Currency must be set.");

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
