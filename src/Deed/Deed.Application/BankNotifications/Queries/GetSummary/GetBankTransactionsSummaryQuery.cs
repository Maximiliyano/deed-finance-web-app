using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankNotifications.Responses;

namespace Deed.Application.BankNotifications.Queries.GetSummary;

public sealed record GetBankTransactionsSummaryQuery(
    string? Period = null,
    int? ConnectionId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Provider = null)
    : IQuery<BankTransactionsSummaryResponse>;
