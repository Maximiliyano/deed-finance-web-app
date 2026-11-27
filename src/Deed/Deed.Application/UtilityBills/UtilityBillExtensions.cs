using Deed.Application.UtilityBills.Commands.Create;
using Deed.Application.UtilityBills.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.UtilityBills;

internal static class UtilityBillExtensions
{
    internal static UtilityBillResponse ToResponse(this UtilityBill bill, DateTimeOffset? nowOverride = null, decimal? previousReading = null)
    {
        var now = nowOverride ?? DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);

        var payment = bill.Payments
            .Where(p => !p.IsDeleted && p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)
            .OrderByDescending(p => p.PaymentDate)
            .FirstOrDefault();

        return new UtilityBillResponse(
            bill.Id,
            bill.Name,
            bill.EstimatedAmount,
            bill.Currency.ToString(),
            bill.DueDayOfMonth,
            bill.CapitalId,
            bill.Capital?.Name,
            bill.Capital?.Currency.ToString(),
            bill.CategoryId,
            bill.Category?.Name,
            bill.IsActive,
            payment is not null,
            payment?.Amount,
            payment?.PaymentDate,
            bill.CreatedAt,
            bill.UnitPrice,
            bill.Unit,
            bill.UnitPrice is null ? null : previousReading ?? 0,
            payment?.MeterReading
        );
    }

    internal static IEnumerable<UtilityBillResponse> ToResponses(
        this IEnumerable<UtilityBill> bills,
        DateTimeOffset? nowOverride = null,
        IReadOnlyDictionary<int, decimal>? previousReadings = null)
        => bills.Select(b => b.ToResponse(
            nowOverride,
            previousReadings is not null && previousReadings.TryGetValue(b.Id, out var reading) ? reading : null));

    internal static string? NormalizeUnit(decimal? unitPrice, string? unit)
        => unitPrice is null || string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();

    internal static UtilityBill ToEntity(this CreateUtilityBillCommand cmd)
        => new()
        {
            Name = cmd.Name.Trim(),
            EstimatedAmount = cmd.EstimatedAmount,
            Currency = Enum.Parse<CurrencyType>(cmd.Currency),
            DueDayOfMonth = cmd.DueDayOfMonth,
            CapitalId = cmd.CapitalId,
            CategoryId = cmd.CategoryId,
            IsActive = cmd.IsActive,
            UnitPrice = cmd.UnitPrice,
            Unit = NormalizeUnit(cmd.UnitPrice, cmd.Unit)
        };
}
