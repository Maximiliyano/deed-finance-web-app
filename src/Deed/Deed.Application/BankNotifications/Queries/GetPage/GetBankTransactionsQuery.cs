using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankNotifications.Responses;

namespace Deed.Application.BankNotifications.Queries.GetPage;

public sealed record GetBankTransactionsQuery(int Page = 1, int PageSize = 20, string? Type = null)
    : IQuery<BankTransactionsPageResponse>;
