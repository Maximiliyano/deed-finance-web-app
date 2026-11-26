using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Queries.Get;

internal sealed class GetBankConnectionQueryHandler(IUser user, IBankConnectionRepository repository)
    : IQueryHandler<GetBankConnectionQuery, BankConnectionResponse>
{
    public async Task<Result<BankConnectionResponse>> Handle(GetBankConnectionQuery query, CancellationToken cancellationToken)
    {
        var connection = await repository
            .GetAsync(new BankConnectionByUserSpecification(user.Name!), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(connection.ToResponse());
    }
}
