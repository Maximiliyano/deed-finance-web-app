using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetAsync(string createdBy, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSettings>> GetAllWithRemindersEnabledAsync(CancellationToken cancellationToken = default);

    void Create(UserSettings settings);

    void Update(UserSettings settings);
}
