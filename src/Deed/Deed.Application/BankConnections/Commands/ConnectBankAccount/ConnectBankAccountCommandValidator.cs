using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectBankAccount;

internal sealed class ConnectBankAccountCommandValidator : AbstractValidator<ConnectBankAccountCommand>
{
    public ConnectBankAccountCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0);

        RuleFor(c => c.Label)
            .MaximumLength(64);
    }
}
