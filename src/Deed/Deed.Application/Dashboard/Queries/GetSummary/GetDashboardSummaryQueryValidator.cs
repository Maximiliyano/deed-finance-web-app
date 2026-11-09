using Deed.Domain.Enums;
using FluentValidation;

namespace Deed.Application.Dashboard.Queries.GetSummary;

internal sealed class GetDashboardSummaryQueryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryQueryValidator()
    {
        RuleFor(q => q.Currency)
            .Must(currency => Enum.TryParse<CurrencyType>(currency, out var parsed) && parsed != CurrencyType.None)
            .When(q => q.Currency is not null);

        RuleFor(q => q.PeriodEnd)
            .GreaterThan(q => q.PeriodStart)
            .When(q => q.PeriodStart.HasValue && q.PeriodEnd.HasValue);
    }
}
