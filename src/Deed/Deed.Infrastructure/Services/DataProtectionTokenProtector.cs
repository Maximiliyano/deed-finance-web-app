using System.Security.Cryptography;
using Deed.Application.Abstractions.Services;
using Microsoft.AspNetCore.DataProtection;

namespace Deed.Infrastructure.Services;

internal sealed class DataProtectionTokenProtector(IDataProtectionProvider provider) : ITokenProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("Deed.BankConnection.Token");

    public string Protect(string token) => _protector.Protect(token);

    public string? Unprotect(string protectedToken)
    {
        try
        {
            return _protector.Unprotect(protectedToken);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
