namespace Deed.Application.Abstractions.Settings;

public sealed class SmtpSettings
{
    public required string Host { get; init; }

    public int Port { get; init; } = 587;

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string FromAddress { get; init; }
}
