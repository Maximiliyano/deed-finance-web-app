namespace Deed.Domain.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateTime MinValue { get; }
}
