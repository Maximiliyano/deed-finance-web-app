using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Deed.Application.Abstractions.Settings;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.BankConnections.Service;

public sealed class PrivatCardService(
    HttpClient client,
    IOptions<PrivatCardSettings> options)
    : IPrivatCardService
{
    private static readonly string[] IdentityAttributes = ["card", "appcode", "trandate", "trantime", "terminal", "description"];

    private static readonly string[] AuthErrorMarkers = ["sign", "merchant", "password", "auth", "access", "denied"];

    public async Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        PrivatCardCredentials credentials,
        string cardNumber,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            using var content = new StringContent(BuildRequest(credentials, cardNumber, from, to), Encoding.UTF8, "text/xml");
            using var response = await client.PostAsync(options.Value.Url, content, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid);
            }

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Privat24 card request failed with status {Status}", (int)response.StatusCode);
                return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return Parse(body);
        }
        catch (Exception e) when (e is HttpRequestException or XmlException or TaskCanceledException)
        {
            Log.Warning(e, "Exception occurred while fetching Privat24 card statements");
            return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution);
        }
    }

    // The request layout and signature follow the legacy p24api rest_fiz interface and must be verified against the bank documentation.
    internal static string BuildRequest(PrivatCardCredentials credentials, string cardNumber, DateTimeOffset from, DateTimeOffset to)
    {
        var start = KyivTime.ToLocal(from).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var end = KyivTime.ToLocal(to).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

        var data = "<oper>cmt</oper><wait>0</wait><test>0</test><payment id=\"\">"
            + $"<prop name=\"sd\" value=\"{start}\" />"
            + $"<prop name=\"ed\" value=\"{end}\" />"
            + $"<prop name=\"card\" value=\"{Escape(cardNumber)}\" />"
            + "</payment>";

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><request version=\"1.0\"><merchant>"
            + $"<id>{Escape(credentials.MerchantId)}</id><signature>{Sign(data, credentials.Password)}</signature>"
            + $"</merchant><data>{data}</data></request>";
    }

    internal static string Sign(string data, string password)
    {
        var payload = Encoding.UTF8.GetBytes(data + password);

#pragma warning disable CA5351, CA5350, S4790
        var md5 = Convert.ToHexStringLower(MD5.HashData(payload));

        return Convert.ToHexStringLower(SHA1.HashData(Encoding.UTF8.GetBytes(md5)));
#pragma warning restore CA5351, CA5350, S4790
    }

    internal static Result<IReadOnlyList<BankTransaction>> Parse(string body)
    {
        XDocument document;

        using (var reader = XmlReader.Create(new StringReader(body), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
        {
            document = XDocument.Load(reader);
        }

        var error = document.Descendants("error").FirstOrDefault();

        if (error is not null)
        {
            var message = (string?)error.Attribute("message") ?? error.Value;
            Log.Warning("Privat24 card request was rejected: {Message}", message);

            return AuthErrorMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase))
                ? Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid)
                : Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution);
        }

        var transactions = document.Descendants("statement").Select(Map).OfType<BankTransaction>().ToList();

        return Result.Success<IReadOnlyList<BankTransaction>>(transactions);
    }

    private static BankTransaction? Map(XElement statement)
    {
        var amountText = ((string?)statement.Attribute("amount"))?.Trim();

        if (string.IsNullOrEmpty(amountText))
        {
            return null;
        }

        var parts = amountText.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (!decimal.TryParse(parts[0].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount == 0)
        {
            return null;
        }

        var date = (string?)statement.Attribute("trandate");
        var time = (string?)statement.Attribute("trantime");
        var terminal = Clean((string?)statement.Attribute("terminal"));
        var description = Clean((string?)statement.Attribute("description"));

        return new BankTransaction(
            BuildExternalId(statement, amountText),
            amount < 0 ? BankTransactionDirection.Outgoing : BankTransactionDirection.Incoming,
            Math.Abs(amount),
            parts.Length > 1 ? parts[1].Trim() : string.Empty,
            terminal,
            description,
            ParseOccurredAt(date, time));
    }

    private static DateTimeOffset ParseOccurredAt(string? date, string? time)
        => DateTime.TryParseExact($"{date} {time}".Trim(), ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? KyivTime.ToOffset(local)
            : DateTimeOffset.UtcNow;

    private static string BuildExternalId(XElement statement, string amountText)
    {
        var key = string.Join('|', IdentityAttributes.Select(name => (string?)statement.Attribute(name) ?? string.Empty).Append(amountText));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        return Convert.ToHexStringLower(hash)[..32];
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Escape(string value)
        => System.Security.SecurityElement.Escape(value) ?? string.Empty;
}
