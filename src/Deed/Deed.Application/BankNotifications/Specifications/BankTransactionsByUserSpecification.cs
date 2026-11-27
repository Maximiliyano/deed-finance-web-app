using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankTransactionsByUserSpecification : BaseSpecification<BankNotification>
{
#pragma warning disable CA1862
    public BankTransactionsByUserSpecification(
        string createdBy,
        BankTransactionType? type = null,
        int? skip = null,
        int? take = null,
        BankImportStatus? importStatus = null,
        int? connectionId = null,
        IReadOnlyCollection<int>? connectionIds = null,
        bool includeUnassigned = true,
        string? currency = null,
        string? search = null,
        decimal? amount = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
        : base(n =>
            n.CreatedBy == createdBy &&
            n.Kind == BankNotificationKind.Transaction &&
            (type == null || n.Type == type) &&
            (importStatus == null || n.ImportStatus == importStatus) &&
            (connectionId == null || n.BankConnectionId == connectionId) &&
            (connectionIds == null || n.BankConnectionId == null && includeUnassigned || n.BankConnectionId != null && connectionIds.Contains(n.BankConnectionId.Value)) &&
            (currency == null || n.Currency == currency) &&
            (from == null || n.OccurredAt >= from) &&
            (to == null || n.OccurredAt < to) &&
            (search == null ||
                n.Counterparty != null && n.Counterparty.ToUpperInvariant().Contains(search) ||
                n.Purpose != null && n.Purpose.ToUpperInvariant().Contains(search) ||
                n.AccountLabel != null && n.AccountLabel.ToUpperInvariant().Contains(search) ||
                amount != null && n.Amount == amount))
#pragma warning restore CA1862
    {
        if (take is null)
        {
            return;
        }

        ApplyOrderByDescending(n => n.OccurredAt);
        ApplyThenByDescending(n => n.Id);
        Skip = skip;
        Take = take;
    }
}
