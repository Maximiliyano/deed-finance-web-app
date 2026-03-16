using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Transfers.Responses;
using Deed.Application.Transfers.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Transfers.Queries.GetAll;

internal sealed class GetAllTransfersQueryHandler(ITransferRepository repository, IUser user)
    : IQueryHandler<GetAllTransfersQuery, IEnumerable<TransferResponse>>
{
    public async Task<Result<IEnumerable<TransferResponse>>> Handle(
        GetAllTransfersQuery query,
        CancellationToken cancellationToken)
    {
        var transfers = await repository
            .GetAllAsync(new TransfersByUserSpecification(user.Name!), cancellationToken)
            .ConfigureAwait(false);

        var responses = transfers.Select(t => new TransferResponse(
            t.Id,
            t.Amount,
            t.DestinationAmount,
            t.SourceCapitalId,
            t.SourceCapital?.Name,
            t.SourceCapital?.Currency.ToString(),
            t.DestinationCapitalId,
            t.DestinationCapital?.Name,
            t.DestinationCapital?.Currency.ToString(),
            t.CreatedAt));

        return Result.Success(responses);
    }
}
