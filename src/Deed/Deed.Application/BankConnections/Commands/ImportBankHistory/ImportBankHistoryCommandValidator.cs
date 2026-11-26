using FluentValidation;

namespace Deed.Application.BankConnections.Commands.ImportBankHistory;

internal sealed class ImportBankHistoryCommandValidator : AbstractValidator<ImportBankHistoryCommand>
{
    internal const int MaxHistoryDays = 1100;

    public ImportBankHistoryCommandValidator()
    {
        RuleFor(c => c.From)
            .GreaterThanOrEqualTo(_ => DateTimeOffset.UtcNow.AddDays(-MaxHistoryDays))
            .WithMessage($"The period can go back at most {MaxHistoryDays / 365} years.");

        RuleFor(c => c.To)
            .GreaterThan(c => c.From)
            .When(c => c.To is not null)
            .WithMessage("The end of the period must be after its start.");
    }
}
