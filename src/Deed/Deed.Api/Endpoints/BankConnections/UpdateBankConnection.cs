using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.UpdateBankConnection;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class UpdateBankConnection : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/bank-connections/{id:int}", async (int id, UpdateBankConnectionRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateBankConnectionCommand(id, request.Label, request.CapitalId, request.AutoImport, request.ExpenseCategoryId, request.IncomeCategoryId), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
