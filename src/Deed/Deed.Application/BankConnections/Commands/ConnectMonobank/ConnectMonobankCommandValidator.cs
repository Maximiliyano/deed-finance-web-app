using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectMonobank;

internal sealed class ConnectMonobankCommandValidator : AbstractValidator<ConnectMonobankCommand>
{
    public ConnectMonobankCommandValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.AccountId)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(c => c.Iban)
            .MaximumLength(34);

        RuleFor(c => c.Label)
            .MaximumLength(64);
    }
}
