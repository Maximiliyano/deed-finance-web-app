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

public sealed class MonobankService(
    HttpClient client,
    IOptions<MonobankPersonalSettings> options)
    : IMonobankService
{
    private const string IdPrefix = "mono:";
    private const decimal MinorUnits = 100m;
    private const int StatementLimit = 500;

    public BankProvider Provider => BankProvider.Monobank;

    public int? MaxItemsPerRequest => StatementLimit;

    public int MaxChunksPerRun => 1;

    public TimeSpan ChunkDelay => TimeSpan.Zero;

    public async Task<Result<IReadOnlyList<MonobankAccount>>> GetAccountsAsync(string token, CancellationToken cancellationToken)
    {
        var body = await GetAsync(token, "personal/client-info", cancellationToken).ConfigureAwait(false);

        if (!body.IsSuccess)
        {
            return Result.Failure<IReadOnlyList<MonobankAccount>>(body.Errors);
        }

        try
        {
            using var document = JsonDocument.Parse(body.Value);

            if (!document.RootElement.TryGetProperty("accounts", out var accounts) || accounts.ValueKind != JsonValueKind.Array)
            {
                return Result.Success<IReadOnlyList<MonobankAccount>>([]);
            }

            var result = accounts.EnumerateArray().Select(MapAccount).OfType<MonobankAccount>().ToList();

            return Result.Success<IReadOnlyList<MonobankAccount>>(result);
        }
        catch (JsonException e)
        {
            Log.Warning(e, "Unexpected Monobank client-info response");
            return Result.Failure<IReadOnlyList<MonobankAccount>>(DomainErrors.HttpClient.Serialization);
        }
    }

    public async Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        string token,
        string account,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var path = $"personal/statement/{Uri.EscapeDataString(account)}/{from.ToUnixTimeSeconds()}/{to.ToUnixTimeSeconds()}";
        var body = await GetAsync(token, path, cancellationToken).ConfigureAwait(false);

        if (!body.IsSuccess)
        {
            return Result.Failure<IReadOnlyList<BankTransaction>>(body.Errors);
        }

        try
        {
            using var document = JsonDocument.Parse(body.Value);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Serialization);
            }

            var transactions = document.RootElement.EnumerateArray().Select(MapTransaction).OfType<BankTransaction>().ToList();

            return Result.Success<IReadOnlyList<BankTransaction>>(transactions);
        }
        catch (JsonException e)
        {
            Log.Warning(e, "Unexpected Monobank statement response");
            return Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Serialization);
        }
    }

    private async Task<Result<string>> GetAsync(string token, string path, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{options.Value.BaseUrl.TrimEnd('/')}/{path}");
            request.Headers.Add("X-Token", token);

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Result.Failure<string>(DomainErrors.Bank.TokenInvalid);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return Result.Failure<string>(DomainErrors.Bank.RateLimited);
            }

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Monobank request failed with status {Status}", (int)response.StatusCode);
                return Result.Failure<string>(DomainErrors.HttpClient.Execution);
            }

            return Result.Success(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            Log.Warning(e, "Exception occurred while calling Monobank");
            return Result.Failure<string>(DomainErrors.HttpClient.Execution);
        }
    }

    private static MonobankAccount? MapAccount(JsonElement item)
    {
        var id = ReadString(item, "id");

        if (id is null)
        {
            return null;
        }

        var masked = item.TryGetProperty("maskedPan", out var pans) && pans.ValueKind == JsonValueKind.Array
            ? pans.EnumerateArray().Select(p => p.GetString()).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))
            : null;

        return new MonobankAccount(
            id,
            ReadString(item, "iban") ?? id,
            CurrencyName(ReadInt(item, "currencyCode")),
            masked,
            ReadString(item, "type") ?? string.Empty,
            ReadLong(item, "balance") / MinorUnits);
    }

    private static BankTransaction? MapTransaction(JsonElement item)
    {
        var id = ReadString(item, "id");
        var amount = ReadLong(item, "amount");

        if (id is null || amount == 0)
        {
            return null;
        }

        var counterName = ReadString(item, "counterName");
        var description = ReadString(item, "description");
        var comment = ReadString(item, "comment");

        return new BankTransaction(
            $"{IdPrefix}{id}",
            amount > 0 ? BankTransactionDirection.Incoming : BankTransactionDirection.Outgoing,
            Math.Abs(amount) / MinorUnits,
            CurrencyName(ReadInt(item, "currencyCode")),
            counterName ?? description,
            comment ?? (counterName is null ? null : description),
            DateTimeOffset.FromUnixTimeSeconds(ReadLong(item, "time")));
    }

    private static string CurrencyName(int code)
        => code switch
        {
            980 => "UAH",
            840 => "USD",
            978 => "EUR",
            985 => "PLN",
            203 => "CZK",
            _ => code.ToString(CultureInfo.InvariantCulture)
        };

    private static long ReadLong(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number)
            ? number
            : 0;

    private static int ReadInt(JsonElement element, string name)
        => (int)ReadLong(element, name);

    private static string? ReadString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();

        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
