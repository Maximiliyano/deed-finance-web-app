using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Queries.GetAll;

public sealed record GetBankConnectionsQuery : IQuery<IReadOnlyList<BankConnectionResponse>>;
