using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class BankNotificationRepository(IDeedDbContext context)
    : GeneralRepository<BankNotification>(context), IBankNotificationRepository;
