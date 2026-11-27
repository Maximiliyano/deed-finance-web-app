namespace Deed.Application.Abstractions.Settings;

public sealed class RateLimitSettings
{
    public int ApiPermitsPerMinute { get; init; } = 300;

    public int BankPermitsPerMinute { get; init; } = 10;

    public int AuthPermitsPerMinute { get; init; } = 10;
}
