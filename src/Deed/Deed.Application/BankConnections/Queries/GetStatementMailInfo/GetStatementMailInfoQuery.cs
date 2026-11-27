using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Queries.GetStatementMailInfo;

public sealed record GetStatementMailInfoQuery : IQuery<StatementMailInfoResponse>;
