using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Statements;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;

namespace Deed.Application.BankConnections.Queries.GetStatementMailInfo;

internal sealed class GetStatementMailInfoQueryHandler(IOptions<StatementMailSettings> options)
    : IQueryHandler<GetStatementMailInfoQuery, StatementMailInfoResponse>
{
    public Task<Result<StatementMailInfoResponse>> Handle(GetStatementMailInfoQuery query, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        var response = settings.IsEnabled
            ? new StatementMailInfoResponse(true, StatementMailAddress.Template(settings.Address))
            : new StatementMailInfoResponse(false, null);

        return Task.FromResult(Result.Success(response));
    }
}
