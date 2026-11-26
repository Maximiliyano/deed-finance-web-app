using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class TagRepository(IDeedDbContext context)
    : GeneralRepository<Tag>(context), ITagRepository;
