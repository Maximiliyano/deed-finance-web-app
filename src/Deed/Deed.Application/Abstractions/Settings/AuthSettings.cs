namespace Deed.Application.Abstractions.Settings;

public sealed class AuthSettings
{
    public required string Domain { get; init; }

    public required string ClientID { get; init; }

    public required string ClientSecret { get; init; }
}
