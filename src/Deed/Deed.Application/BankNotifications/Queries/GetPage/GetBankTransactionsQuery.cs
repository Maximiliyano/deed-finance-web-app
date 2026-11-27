using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankNotifications.Responses;

namespace Deed.Application.BankNotifications.Queries.GetPage;

public sealed record GetBankTransactionsQuery(int Page = 1, int PageSize = 20, string? Type = null, string? Status = null, int? ConnectionId = null, string? Provider = null, string? Currency = null, string? Search = null, DateTimeOffset? From = null, DateTimeOffset? To = null)
    : IQuery<BankTransactionsPageResponse>;
