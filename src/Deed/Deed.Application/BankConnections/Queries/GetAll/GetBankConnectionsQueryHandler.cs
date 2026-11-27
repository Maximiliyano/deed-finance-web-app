using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Queries.GetAll;

internal sealed class GetBankConnectionsQueryHandler(IUser user, IBankConnectionRepository repository)
    : IQueryHandler<GetBankConnectionsQuery, IReadOnlyList<BankConnectionResponse>>
{
    public async Task<Result<IReadOnlyList<BankConnectionResponse>>> Handle(GetBankConnectionsQuery query, CancellationToken cancellationToken)
    {
        var connections = await repository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Id), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(connections.ToResponses());
    }
}
