using Deed.Application.UtilityBills.Commands.Create;
using Deed.Application.UtilityBills.Commands.Update;
using Deed.Application.UtilityBills.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.UtilityBills;

internal static class UtilityBillExtensions
{
    internal static UtilityBillResponse ToResponse(this UtilityBill bill, DateTimeOffset? nowOverride = null)
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
            bill.CategoryId,
            bill.Category?.Name,
            bill.IsActive,
            payment is not null,
            payment?.Amount,
            payment?.PaymentDate,
            payment?.Id
        );
    }

    internal static IEnumerable<UtilityBillResponse> ToResponses(this IEnumerable<UtilityBill> bills, DateTimeOffset? nowOverride = null)
        => bills.Select(b => b.ToResponse(nowOverride));

    internal static UtilityBill ToEntity(this CreateUtilityBillCommand cmd)
        => new()
        {
            Name = cmd.Name.Trim(),
            EstimatedAmount = cmd.EstimatedAmount,
            Currency = cmd.Currency,
            DueDayOfMonth = cmd.DueDayOfMonth,
            CapitalId = cmd.CapitalId,
            CategoryId = cmd.CategoryId,
            IsActive = cmd.IsActive
        };

    internal static void ApplyUpdate(this UtilityBill bill, UpdateUtilityBillCommand cmd)
    {
        bill.Name = cmd.Name.Trim();
        bill.EstimatedAmount = cmd.EstimatedAmount;
        bill.Currency = cmd.Currency;
        bill.DueDayOfMonth = cmd.DueDayOfMonth;
        bill.CapitalId = cmd.CapitalId;
        bill.CategoryId = cmd.CategoryId;
        bill.IsActive = cmd.IsActive;
    }
}
