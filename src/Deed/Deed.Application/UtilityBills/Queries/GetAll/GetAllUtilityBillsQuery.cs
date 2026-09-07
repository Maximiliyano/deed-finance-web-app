using Deed.Application.Abstractions.Messaging;
using Deed.Application.UtilityBills.Responses;

namespace Deed.Application.UtilityBills.Queries.GetAll;

public sealed record GetAllUtilityBillsQuery : IQuery<IEnumerable<UtilityBillResponse>>;
