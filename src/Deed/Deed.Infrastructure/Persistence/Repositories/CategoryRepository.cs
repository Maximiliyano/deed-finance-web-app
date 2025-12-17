using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(IDeedDbContext context)
    : GeneralRepository<Category>(context), ICategoryRepository;
