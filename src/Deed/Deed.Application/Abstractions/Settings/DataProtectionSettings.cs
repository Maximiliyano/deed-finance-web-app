namespace Deed.Application.Abstractions.Settings;

public sealed class DataProtectionSettings
{
    public string? CertificateBase64 { get; init; }

    public string? CertificatePassword { get; init; }

    public bool ReprotectExistingKeys { get; init; }

    public bool HasCertificate => !string.IsNullOrWhiteSpace(CertificateBase64);
}
