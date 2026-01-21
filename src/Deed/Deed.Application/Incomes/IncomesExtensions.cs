using Deed.Application.Incomes.Commands.Create;
using Deed.Application.Incomes.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.Incomes;

internal static class IncomesExtensions
{
    internal static IncomeResponse ToResponse(this Income i)
        => new(
            i.Id,
            i.CapitalId,
            i.CategoryId,
            i.Amount,
            i.PaymentDate,
            i.Purpose
        );

    internal static IEnumerable<IncomeResponse> ToResponses(this IEnumerable<Income> entities)
        => entities.Select(e => e.ToResponse());

    internal static Income ToEntity(this CreateIncomeCommand command)
        => new()
        {
            Amount = command.Amount,
            Purpose = command.Purpose,
            CategoryId = command.CategoryId,
            CapitalId = command.CapitalId,
            PaymentDate = command.PaymentDate
        };
}
