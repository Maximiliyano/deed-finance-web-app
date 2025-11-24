using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class IncomeRepository(IDeedDbContext context)
    : GeneralRepository<Income>(context), IIncomeRepository;
