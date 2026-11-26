using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ConnectPrivat;

internal sealed class ConnectPrivatCommandValidator : AbstractValidator<ConnectPrivatCommand>
{
    public ConnectPrivatCommandValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(c => c.Iban)
            .NotEmpty()
            .Must(iban => BankConnectionExtensions.NormalizeIban(iban).Length == 29
                && BankConnectionExtensions.NormalizeIban(iban).StartsWith("UA", StringComparison.Ordinal)
                && BankConnectionExtensions.NormalizeIban(iban)[2..].All(char.IsAsciiLetterOrDigit))
            .WithMessage("The IBAN must look like UA followed by 27 characters.");
    }
}
