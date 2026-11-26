namespace Deed.Application.Capitals.Requests;

public sealed record UpdateCapitalOrdersRequest(
    IEnumerable<CapitalOrder> Capitals
);
