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
            .Must(iban => string.IsNullOrWhiteSpace(iban) || IsValidIban(BankConnectionExtensions.NormalizeIban(iban)))
            .WithMessage("The IBAN must look like UA followed by 27 characters, or be left empty to watch all accounts.");
    }

    private static bool IsValidIban(string iban)
    {
        return iban.Length == 29
            && iban.StartsWith("UA", StringComparison.Ordinal)
            && iban[2..].All(char.IsAsciiLetterOrDigit);
    }
}
