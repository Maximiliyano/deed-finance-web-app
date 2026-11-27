using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Queries.GetBankAccounts;

internal sealed class GetBankAccountsQueryHandler(
    IUser user,
    IBankAccountRepository accountRepository,
    IBankConnectionRepository connectionRepository)
    : IQueryHandler<GetBankAccountsQuery, IReadOnlyList<LinkedBankAccountResponse>>
{
    public async Task<Result<IReadOnlyList<LinkedBankAccountResponse>>> Handle(GetBankAccountsQuery query, CancellationToken cancellationToken)
    {
        var accounts = await accountRepository
            .GetAllAsync(new BankAccountsByUserSpecification(user.Id), cancellationToken)
            .ConfigureAwait(false);

        var connections = (await connectionRepository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Id), cancellationToken)
            .ConfigureAwait(false)).ToList();

        IReadOnlyList<LinkedBankAccountResponse> response =
        [
            .. accounts.Select(a => new LinkedBankAccountResponse(
                a.Id,
                a.Provider.ToString(),
                a.Iban,
                BankConnectionExtensions.MaskAccount(a.Iban),
                a.Currency,
                a.Name,
                a.Balance,
                a.LastRefreshedAt,
                connections.FirstOrDefault(c => c.Provider == a.Provider && string.Equals(c.AccountIban, a.Iban, StringComparison.OrdinalIgnoreCase))?.Id))
        ];

        return Result.Success(response);
    }
}
