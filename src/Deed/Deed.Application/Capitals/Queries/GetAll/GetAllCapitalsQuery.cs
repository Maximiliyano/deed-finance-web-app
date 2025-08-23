using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Responses;

namespace Deed.Application.Capitals.Queries.GetAll;

public sealed record GetAllCapitalsQuery(
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortDirection = null)
    : IQuery<IEnumerable<CapitalResponse>>;
