using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository(IDeedDbContext context)
    : GeneralRepository<Transfer>(context), ITransferRepository;
