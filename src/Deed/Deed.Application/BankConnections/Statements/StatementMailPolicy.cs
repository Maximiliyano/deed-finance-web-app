using Deed.Application.Abstractions.Settings;

namespace Deed.Application.BankConnections.Statements;

internal static class StatementMailPolicy
{
    internal static bool IsTrusted(string? fromAddress, string? authenticationResults, StatementMailSettings settings)
    {
        if (!IsAllowedDomain(DomainOf(fromAddress), settings.AllowedSenders))
        {
            return false;
        }

        return !settings.RequireAuthenticationResults || HasPassingResult(authenticationResults, settings);
    }

    private static bool HasPassingResult(string? header, StatementMailSettings settings)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return false;
        }

        var segments = StatementValues.Collapse(header).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(settings.TrustedAuthServId)
            && !string.Equals(segments[0].Split(' ', 2)[0], settings.TrustedAuthServId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return segments.Skip(1).Any(segment =>
            segment.StartsWith("dkim=pass", StringComparison.OrdinalIgnoreCase)
                && IsAllowedDomain(DomainOf(Property(segment, "header.d") ?? Property(segment, "header.i")), settings.AllowedSenders)
            || segment.StartsWith("spf=pass", StringComparison.OrdinalIgnoreCase)
                && IsAllowedDomain(DomainOf(Property(segment, "smtp.mailfrom")), settings.AllowedSenders));
    }

    private static string? Property(string segment, string name)
    {
        foreach (var token in segment.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
            {
                return token[(name.Length + 1)..].Trim('"');
            }
        }

        return null;
    }

    private static string? DomainOf(string? address)
    {
        var value = address?.Trim().Trim('<', '>', '@');

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var at = value.LastIndexOf('@');

        return (at >= 0 ? value[(at + 1)..] : value).ToUpperInvariant();
    }

    private static bool IsAllowedDomain(string? domain, string[] allowed)
        => domain is not null
            && allowed.Any(entry =>
            {
                var allowedDomain = entry.Trim().TrimStart('@').ToUpperInvariant();

                return allowedDomain.Length > 0
                    && (domain == allowedDomain || domain.EndsWith("." + allowedDomain, StringComparison.Ordinal));
            });
}
