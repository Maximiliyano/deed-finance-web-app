namespace Deed.Domain.Providers;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTime MinValue => DateTime.MinValue;
}
