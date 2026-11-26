using UserSettingsEntity = Deed.Domain.Entities.UserSettings;

namespace Deed.Application.Reminders;

public interface IReminderDigestService
{
    Task<bool> SendAsync(UserSettingsEntity settings, ReminderDigestRequest request, DateTimeOffset now, CancellationToken cancellationToken = default);
}

public sealed record ReminderDigestRequest(bool IncludeBalance, bool IncludeDebts, DateTimeOffset? ActivitySince);
