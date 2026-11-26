using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Queries.Get;

public sealed record GetBankConnectionQuery : IQuery<BankConnectionResponse>;
