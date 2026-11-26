namespace Deed.Application.Debts.Requests;

public sealed record UpdateDebtOrdersRequest(
    IEnumerable<DebtOrder> Debts
);
