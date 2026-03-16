using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class UserSettingsRepository(IDeedDbContext context) : IUserSettingsRepository
{
    public async Task<UserSettings?> GetAsync(string createdBy, CancellationToken cancellationToken = default)
        => await context.Set<UserSettings>()
            .SingleOrDefaultAsync(s => s.CreatedBy == createdBy, cancellationToken);

    public async Task<IReadOnlyList<UserSettings>> GetAllWithRemindersEnabledAsync(CancellationToken cancellationToken = default)
        => await context.Set<UserSettings>()
            .AsNoTracking()
            .Where(s => s.BalanceReminderEnabled || s.ExpenseReminderEnabled || s.DebtReminderEnabled)
            .ToListAsync(cancellationToken);

    public void Create(UserSettings settings)
        => context.Set<UserSettings>().Add(settings);

    public void Update(UserSettings settings)
        => context.Set<UserSettings>().Update(settings);
}
