using Deed.Api.Extensions;
using Deed.Application.Debts.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Debts;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/debts", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAllDebtsQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(Debts));
    }
}
