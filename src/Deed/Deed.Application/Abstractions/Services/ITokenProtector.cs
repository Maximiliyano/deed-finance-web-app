namespace Deed.Application.Abstractions.Services;

public interface ITokenProtector
{
    string Protect(string token);

    string? Unprotect(string protectedToken);
}
