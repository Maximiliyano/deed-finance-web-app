namespace Deed.Application.Abstractions.Data;

public sealed record QueryParams(
    string? SearchTerm,
    string? SortBy,
    string? SortDirection,
    string? FilterBy
);
