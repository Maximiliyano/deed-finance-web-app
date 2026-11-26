using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.SetBankConnectionExcluded;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class SetBankConnectionExcluded : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/bank-connections/{id:int}/excluded", async (int id, SetBankConnectionExcludedRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SetBankConnectionExcludedCommand(id, request.Excluded), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
