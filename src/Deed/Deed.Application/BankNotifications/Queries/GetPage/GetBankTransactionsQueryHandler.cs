using System.Globalization;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Responses;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Queries.GetPage;

internal sealed class GetBankTransactionsQueryHandler(
    IUser user,
    IBankNotificationRepository repository,
    IBankConnectionRepository connectionRepository)
    : IQueryHandler<GetBankTransactionsQuery, BankTransactionsPageResponse>
{
    private const int MaxPageSize = 100;

    public async Task<Result<BankTransactionsPageResponse>> Handle(GetBankTransactionsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        BankTransactionType? type = Enum.TryParse<BankTransactionType>(query.Type, true, out var parsed) && parsed != BankTransactionType.None
            ? parsed
            : null;

        BankImportStatus? status = Enum.TryParse<BankImportStatus>(query.Status, true, out var parsedStatus) && parsedStatus != BankImportStatus.None
            ? parsedStatus
            : null;

        var currency = string.IsNullOrWhiteSpace(query.Currency) ? null : query.Currency.Trim().ToUpperInvariant();

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim().ToUpperInvariant();
        decimal? amount = search is not null && decimal.TryParse(search.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount)
            ? Math.Abs(parsedAmount)
            : null;

        var connections = await connectionRepository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Id), cancellationToken)
            .ConfigureAwait(false);

        var (connectionIds, includeUnassigned) = BankConnectionScope.Resolve(connections, query.Provider, query.ConnectionId);

        var total = await repository
            .CountAsync(new BankTransactionsByUserSpecification(user.Id, type, importStatus: status, connectionId: query.ConnectionId, connectionIds: connectionIds, includeUnassigned: includeUnassigned, currency: currency, search: search, amount: amount, from: query.From, to: query.To), cancellationToken)
            .ConfigureAwait(false);

        var items = await repository
            .GetAllAsync(new BankTransactionsByUserSpecification(user.Id, type, (page - 1) * pageSize, pageSize, status, query.ConnectionId, connectionIds, includeUnassigned, currency, search, amount, query.From, query.To), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new BankTransactionsPageResponse(total, page, pageSize, items.ToResponses()));
    }
}
