using Deed.Application.Abstractions.Messaging;
using Deed.Application.Debts.Responses;

namespace Deed.Application.Debts.Queries.GetAll;

public sealed record GetAllDebtsQuery : IQuery<IEnumerable<DebtResponse>>;
