using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Dashboard;

public sealed record GetDashboardQuery : IQuery<DashboardResponse>;
