using FluentValidation;

namespace Deed.Application.BankConnections.Queries.GetMonobankAccounts;

internal sealed class GetMonobankAccountsQueryValidator : AbstractValidator<GetMonobankAccountsQuery>
{
    public GetMonobankAccountsQueryValidator()
    {
        RuleFor(q => q.Token)
            .NotEmpty()
            .MaximumLength(256);
    }
}
