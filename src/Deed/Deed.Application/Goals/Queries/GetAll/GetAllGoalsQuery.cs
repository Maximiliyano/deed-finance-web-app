using Deed.Application.Abstractions.Messaging;
using Deed.Application.Goals.Responses;

namespace Deed.Application.Goals.Queries.GetAll;

public sealed record GetAllGoalsQuery : IQuery<IEnumerable<GoalResponse>>;
