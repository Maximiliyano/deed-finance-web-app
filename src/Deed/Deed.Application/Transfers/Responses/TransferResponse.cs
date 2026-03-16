namespace Deed.Application.Transfers.Responses;

public sealed record TransferResponse(
    int Id,
    decimal Amount,
    decimal DestinationAmount,
    int SourceCapitalId,
    string? SourceCapitalName,
    string? SourceCurrency,
    int DestinationCapitalId,
    string? DestinationCapitalName,
    string? DestinationCurrency,
    DateTimeOffset CreatedAt);
