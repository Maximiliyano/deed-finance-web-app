using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class DebtRepository(IDeedDbContext context)
    : GeneralRepository<Debt>(context), IDebtRepository;
