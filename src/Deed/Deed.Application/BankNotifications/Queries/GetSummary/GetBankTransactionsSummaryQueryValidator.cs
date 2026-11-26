using FluentValidation;

namespace Deed.Application.BankNotifications.Queries.GetSummary;

internal sealed class GetBankTransactionsSummaryQueryValidator : AbstractValidator<GetBankTransactionsSummaryQuery>
{
    private const int MaxYears = 5;

    public GetBankTransactionsSummaryQueryValidator()
    {
        RuleFor(q => q.Period)
            .Must(period => period is null || Enum.TryParse<BankSummaryPeriod>(period, true, out _))
            .WithMessage("The period must be day, month or year.");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.From < q.To)
            .WithMessage("The start of the period must be before its end.")
            .Must(q => q.From is null || q.To is null || q.To - q.From <= TimeSpan.FromDays(366 * MaxYears))
            .WithMessage("The period is too long.");
    }
}
