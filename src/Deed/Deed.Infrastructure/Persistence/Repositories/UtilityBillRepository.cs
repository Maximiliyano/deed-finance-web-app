using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class UtilityBillRepository(IDeedDbContext context)
    : GeneralRepository<UtilityBill>(context), IUtilityBillRepository;
