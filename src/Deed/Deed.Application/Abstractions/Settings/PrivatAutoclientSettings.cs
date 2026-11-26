namespace Deed.Application.Abstractions.Settings;

public sealed class PrivatAutoclientSettings
{
    public string BaseUrl { get; init; } = "https://acp.privatbank.ua/api";

    public string TransactionsPath { get; init; } = "statements/transactions/interim";

    public int PageLimit { get; init; } = 100;

    public int MaxPages { get; init; } = 10;
}
