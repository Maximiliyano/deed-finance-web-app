using FluentValidation;

namespace Deed.Application.BankNotifications.Queries.GetPage;

internal sealed class GetBankTransactionsQueryValidator : AbstractValidator<GetBankTransactionsQuery>
{
    private const int MaxSearchLength = 100;

    public GetBankTransactionsQueryValidator()
    {
        RuleFor(q => q.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"The search must be at most {MaxSearchLength} characters.");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.From < q.To)
            .WithMessage("The start of the period must be before its end.");
    }
}
