using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankNotifications.Responses;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Queries.GetPage;

internal sealed class GetBankTransactionsQueryHandler(IUser user, IBankNotificationRepository repository)
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

        var total = await repository
            .CountAsync(new BankTransactionsByUserSpecification(user.Name!, type, importStatus: status), cancellationToken)
            .ConfigureAwait(false);

        var items = await repository
            .GetAllAsync(new BankTransactionsByUserSpecification(user.Name!, type, (page - 1) * pageSize, pageSize, status), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new BankTransactionsPageResponse(total, page, pageSize, items.ToResponses()));
    }
}
