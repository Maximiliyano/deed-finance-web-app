namespace Deed.Application.Abstractions.Settings;

public sealed class PrivatAutoclientSettings
{
    public string BaseUrl { get; init; } = "https://acp.privatbank.ua/api";

    public string TransactionsPath { get; init; } = "statements/transactions";

    public string BalancesPath { get; init; } = "statements/balance/interim";

    public int PageLimit { get; init; } = 100;

    public int MaxPages { get; init; } = 50;

    public int PageDelayMs { get; init; } = 1000;

    public int ChunksPerRun { get; init; } = 6;

    public int ChunkDelayMs { get; init; } = 1000;

    public int SyncIntervalMinutes { get; init; } = 30;
}
