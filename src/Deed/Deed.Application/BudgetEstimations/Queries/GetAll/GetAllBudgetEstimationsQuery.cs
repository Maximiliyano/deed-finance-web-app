using Deed.Application.Abstractions.Messaging;
using Deed.Application.BudgetEstimations.Responses;

namespace Deed.Application.BudgetEstimations.Queries.GetAll;

public sealed record GetAllBudgetEstimationsQuery : IQuery<IEnumerable<BudgetEstimationResponse>>;
