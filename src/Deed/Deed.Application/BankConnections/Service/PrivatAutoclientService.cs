using System.Globalization;
using System.Net;
using System.Text;
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
    static PrivatAutoclientService() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private const string UserAgent = "DeedFinance/1.0";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly string[] DateTimeFormats =
    [
        "dd.MM.yyyy HH:mm:ss",
        "dd.MM.yyyy HH:mm",
        "dd.MM.yyyy"
    ];

    public BankProvider Provider => BankProvider.PrivatAutoclient;

    public int? MaxItemsPerRequest => null;

    public int MaxChunksPerRun => options.Value.ChunksPerRun;

    public TimeSpan ChunkDelay => TimeSpan.FromMilliseconds(options.Value.ChunkDelayMs);

    public TimeSpan MinSyncInterval => TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes);

    public async Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        string token,
        string account,
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
                if (page > 0 && settings.PageDelayMs > 0)
                {
                    await Task.Delay(settings.PageDelayMs, cancellationToken).ConfigureAwait(false);
                }

                var body = await SendAsync(BuildUrl(settings, account, from, to, followId), token, cancellationToken).ConfigureAwait(false);

                if (!body.IsSuccess)
                {
                    return Result.Failure<IReadOnlyList<BankTransaction>>(body.Errors);
                }

                using var document = JsonDocument.Parse(body.Value);

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

    public async Task<Result<IReadOnlyList<PrivatAccount>>> GetAccountsAsync(string token, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var accounts = new Dictionary<string, PrivatAccount>(StringComparer.OrdinalIgnoreCase);
        string? followId = null;
        var lastBody = string.Empty;

        try
        {
            for (var page = 0; page < settings.MaxPages; page++)
            {
                if (page > 0 && settings.PageDelayMs > 0)
                {
                    await Task.Delay(settings.PageDelayMs, cancellationToken).ConfigureAwait(false);
                }

                var url = $"{settings.BaseUrl.TrimEnd('/')}/{settings.BalancesPath.TrimStart('/')}?limit={settings.PageLimit}"
                    + (followId is null ? string.Empty : $"&followId={Uri.EscapeDataString(followId)}");

                var body = await SendAsync(url, token, cancellationToken).ConfigureAwait(false);

                if (!body.IsSuccess)
                {
                    return Result.Failure<IReadOnlyList<PrivatAccount>>(body.Errors);
                }

                lastBody = body.Value;

                using var document = JsonDocument.Parse(body.Value);

                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return Result.Failure<IReadOnlyList<PrivatAccount>>(DomainErrors.HttpClient.Serialization);
                }

                var items = root.TryGetProperty("balances", out var balances) && balances.ValueKind == JsonValueKind.Array
                    ? balances
                    : root.EnumerateObject().Select(p => p.Value).FirstOrDefault(v => v.ValueKind == JsonValueKind.Array);

                if (items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var account in items.EnumerateArray().Select(MapAccount).OfType<PrivatAccount>())
                    {
                        accounts.TryAdd(account.Iban, account);
                    }
                }

                followId = root.TryGetProperty("exist_next_page", out var more) && more.ValueKind == JsonValueKind.True
                    ? ReadString(root, "next_page_id")
                    : null;

                if (followId is null)
                {
                    break;
                }
            }

            if (accounts.Count == 0)
            {
                Log.Warning("Autoclient balance response contained no recognizable accounts, shape: {Shape}", Shape(lastBody));
            }

            return Result.Success<IReadOnlyList<PrivatAccount>>([.. accounts.Values]);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException)
        {
            Log.Warning(e, "Exception occurred while fetching Autoclient accounts");
            return Result.Failure<IReadOnlyList<PrivatAccount>>(DomainErrors.HttpClient.Execution);
        }
    }

    private async Task<Result<string>> SendAsync(string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("token", token);
        request.Headers.UserAgent.ParseAdd(UserAgent);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var body = Decode(bytes);

        if (response.IsSuccessStatusCode)
        {
            return Result.Success(body);
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Log.Warning("Autoclient rejected the token with status {Status}", (int)response.StatusCode);
        }
        else
        {
            Log.Warning("Autoclient request failed with status {Status}: {Body}", (int)response.StatusCode, Preview(body));
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Result.Failure<string>(DomainErrors.Bank.TokenInvalid),
            HttpStatusCode.TooManyRequests => Result.Failure<string>(DomainErrors.Bank.RateLimited),
            _ => Result.Failure<string>(DomainErrors.HttpClient.Execution)
        };
    }

    private static string Preview(string body)
        => body.Length <= 300 ? body : body[..300];

    private static string Shape(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return root.ValueKind.ToString();
            }

            var list = root.EnumerateObject().Select(p => p.Value).FirstOrDefault(v => v.ValueKind == JsonValueKind.Array);
            var item = list.ValueKind == JsonValueKind.Array && list.GetArrayLength() > 0 ? list[0] : default;
            var fields = item.ValueKind == JsonValueKind.Object ? item.EnumerateObject().Select(p => p.Name) : Array.Empty<string>();

            return $"root=[{string.Join(',', root.EnumerateObject().Select(p => p.Name))}] item=[{string.Join(',', fields)}]";
        }
        catch (JsonException)
        {
            return "invalid json";
        }
    }

    private static PrivatAccount? MapAccount(JsonElement item)
    {
        var iban = ReadString(item, "acc") ?? ReadString(item, "ACC") ?? ReadString(item, "iban");

        if (iban is null || !iban.StartsWith("UA", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var balance = ReadString(item, "balanceOut") ?? ReadString(item, "balanceIn");

        return new PrivatAccount(
            iban.ToUpperInvariant(),
            ReadString(item, "currency") ?? ReadString(item, "ccy") ?? string.Empty,
            ReadString(item, "nameACC") ?? ReadString(item, "name"),
            decimal.TryParse(balance?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : null);
    }

    private static string BuildUrl(PrivatAutoclientSettings settings, string iban, DateTimeOffset from, DateTimeOffset to, string? followId)
    {
        var start = KyivTime.ToLocal(from).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        var end = KyivTime.ToLocal(to).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

        var account = string.IsNullOrEmpty(iban) ? string.Empty : $"iban={Uri.EscapeDataString(iban)}&";

        var url = $"{settings.BaseUrl.TrimEnd('/')}/{settings.TransactionsPath.TrimStart('/')}"
            + $"?{account}startDate={start}&endDate={end}&limit={settings.PageLimit}";

        return followId is null ? url : $"{url}&followId={Uri.EscapeDataString(followId)}";
    }

    private static string Decode(byte[] bytes)
    {
        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1251).GetString(bytes);
        }
    }

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
