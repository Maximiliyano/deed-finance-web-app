using Deed.Application.BudgetEstimations.Commands.Create;
using Deed.Application.BudgetEstimations.Commands.Update;
using Deed.Application.BudgetEstimations.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BudgetEstimations;

internal static class BudgetEstimationExtensions
{
    internal static BudgetEstimationResponse ToResponse(this BudgetEstimation est)
        => new(
            est.Id,
            est.Description,
            est.BudgetAmount,
            est.BudgetCurrency.ToString(),
            est.CapitalId,
            est.Capital?.Name,
            est.Capital?.Balance ?? 0m,
            est.Capital?.TotalExpense ?? 0m,
            est.Capital?.Currency.ToString(),
            est.OrderIndex
        );

    internal static IEnumerable<BudgetEstimationResponse> ToResponses(this IEnumerable<BudgetEstimation> list)
        => list.Select(e => e.ToResponse());

    internal static BudgetEstimation ToEntity(this CreateBudgetEstimationCommand cmd)
        => new()
        {
            Description = cmd.Description.Trim(),
            BudgetAmount = cmd.BudgetAmount,
            BudgetCurrency = cmd.BudgetCurrency,
            CapitalId = cmd.CapitalId,
        };

    internal static void ApplyUpdate(this BudgetEstimation est, UpdateBudgetEstimationCommand cmd)
    {
        est.Description = cmd.Description.Trim();
        est.BudgetAmount = cmd.BudgetAmount;
        est.BudgetCurrency = cmd.BudgetCurrency;
        est.CapitalId = cmd.CapitalId;
    }
}
