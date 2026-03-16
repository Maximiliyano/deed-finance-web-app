using Deed.Application.Abstractions.Messaging;
using Deed.Application.Transfers.Responses;

namespace Deed.Application.Transfers.Queries.GetAll;

public sealed record GetAllTransfersQuery() : IQuery<IEnumerable<TransferResponse>>;
