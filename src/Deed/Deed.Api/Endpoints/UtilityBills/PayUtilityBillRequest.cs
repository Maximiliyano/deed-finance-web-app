namespace Deed.Api.Endpoints.UtilityBills;

internal sealed record PayUtilityBillRequest(
    decimal Amount,
    DateTimeOffset? PaymentDate);
