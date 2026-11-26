using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.CancelBankHistory;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class CancelHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bank-connections/history", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new CancelBankHistoryCommand(), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
