using System.Globalization;
using System.Net;
using System.Text.Json;
using Deed.Application.Abstractions.Settings;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.BankConnections.Service;

public sealed class PrivatAutoclientService(
    HttpClient client,
    IOptions<PrivatAutoclientSettings> options)
    : IPrivatAutoclientService
{
    private static readonly string[] DateTimeFormats =
    [
        "dd.MM.yyyy HH:mm:ss",
        "dd.MM.yyyy HH:mm",
        "dd.MM.yyyy"
    ];

    public async Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        string token,
        string iban,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var transactions = new List<BankTransaction>();
        string? followId = null;

        try
        {
            for (var page = 0; page < settings.MaxPages; page++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(settings, iban, from, to, followId));
                request.Headers.Add("token", token);

                using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid);
                }

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Autoclient request failed with status {Status}", (int)response.StatusCode);
                    return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution);
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Serialization);
                }

                if (root.TryGetProperty("transactions", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    transactions.AddRange(items.EnumerateArray().Select(Map).OfType<BankTransaction>());
                }

                followId = root.TryGetProperty("exist_next_page", out var more) && more.ValueKind == JsonValueKind.True
                    ? ReadString(root, "next_page_id")
                    : null;

                if (followId is null)
                {
                    break;
                }
            }

            return Result.Success<IReadOnlyList<BankTransaction>>(transactions);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException)
        {
            Log.Warning(e, "Exception occurred while fetching Autoclient transactions");
            return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution);
        }
    }

    private static string BuildUrl(PrivatAutoclientSettings settings, string iban, DateTimeOffset from, DateTimeOffset to, string? followId)
    {
        var start = KyivTime.ToLocal(from).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        var end = KyivTime.ToLocal(to).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

        var url = $"{settings.BaseUrl.TrimEnd('/')}/{settings.TransactionsPath.TrimStart('/')}"
            + $"?acc={Uri.EscapeDataString(iban)}&startDate={start}&endDate={end}&limit={settings.PageLimit}";

        return followId is null ? url : $"{url}&followId={Uri.EscapeDataString(followId)}";
    }

    // Field names follow the Autoclient statements API and must be verified against the bank documentation.
    private static BankTransaction? Map(JsonElement item)
    {
        var externalId = ReadString(item, "ID") ?? JoinReference(ReadString(item, "REF"), ReadString(item, "REFN"));
        var amount = ReadString(item, "SUM");
        var type = ReadString(item, "TRANTYPE");

        if (externalId is null
            || !decimal.TryParse(amount?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var sum)
            || ParseDirection(type) is not { } direction)
        {
            return null;
        }

        return new BankTransaction(
            externalId,
            direction,
            Math.Abs(sum),
            ReadString(item, "CCY") ?? string.Empty,
            ReadString(item, "AUT_CNTR_NAM"),
            ReadString(item, "OSND"),
            ParseOccurredAt(item));
    }

    private static BankTransactionDirection? ParseDirection(string? type)
        => type?.ToUpperInvariant() switch
        {
            "C" => BankTransactionDirection.Incoming,
            "D" => BankTransactionDirection.Outgoing,
            _ => null
        };

    private static DateTimeOffset ParseOccurredAt(JsonElement item)
    {
        var combined = ReadString(item, "DATE_TIME_DAT_OD_TIM_P")
            ?? string.Join(' ', new[] { ReadString(item, "DAT_OD"), ReadString(item, "TIM_P") }.OfType<string>());

        return DateTime.TryParseExact(combined, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? KyivTime.ToOffset(local)
            : DateTimeOffset.UtcNow;
    }

    private static string? JoinReference(string? reference, string? number)
    {
        if (reference is null)
        {
            return null;
        }

        return number is null ? reference : $"{reference}.{number}";
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        var text = value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };

        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
