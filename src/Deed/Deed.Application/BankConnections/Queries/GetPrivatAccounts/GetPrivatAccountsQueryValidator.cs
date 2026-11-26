using FluentValidation;

namespace Deed.Application.BankConnections.Queries.GetPrivatAccounts;

internal sealed class GetPrivatAccountsQueryValidator : AbstractValidator<GetPrivatAccountsQuery>
{
    public GetPrivatAccountsQueryValidator()
    {
        RuleFor(q => q.Token)
            .NotEmpty()
            .MaximumLength(512);
    }
}
