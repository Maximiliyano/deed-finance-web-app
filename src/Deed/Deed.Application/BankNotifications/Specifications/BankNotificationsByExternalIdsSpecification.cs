using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankNotificationsByExternalIdsSpecification(string createdBy, IReadOnlyCollection<string> externalIds)
    : BaseSpecification<BankNotification>(n => n.CreatedBy == createdBy && externalIds.Contains(n.ExternalId));
