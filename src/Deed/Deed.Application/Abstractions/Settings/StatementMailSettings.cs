namespace Deed.Application.Abstractions.Settings;

public sealed class StatementMailSettings
{
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 993;

    public bool UseSsl { get; init; } = true;

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string Folder { get; init; } = "INBOX";

    public string ProcessedFolder { get; init; } = "Processed";

    public string[] AllowedSenders { get; init; } = ["privatbank.ua"];

    public bool RequireAuthenticationResults { get; init; } = true;

    public string TrustedAuthServId { get; init; } = string.Empty;

    public int MaxAttachmentBytes { get; init; } = 5_000_000;

    public int MaxMessagesPerRun { get; init; } = 50;

    public bool IsEnabled
        => !string.IsNullOrWhiteSpace(Host)
            && !string.IsNullOrWhiteSpace(UserName)
            && !string.IsNullOrWhiteSpace(Password)
            && Address.Contains('@', StringComparison.Ordinal);
}
