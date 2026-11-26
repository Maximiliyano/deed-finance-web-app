using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankNotifications.Responses;

namespace Deed.Application.BankNotifications.Queries.GetAll;

public sealed record GetBankNotificationsQuery(bool UnreadOnly = false, int Limit = 30)
    : IQuery<BankNotificationsResponse>;
