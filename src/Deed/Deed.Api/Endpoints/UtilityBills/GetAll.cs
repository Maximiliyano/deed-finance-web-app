using Deed.Api.Extensions;
using Deed.Application.UtilityBills.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/utility-bills", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAllUtilityBillsQuery(), ct)).Process())
            .AllowAnonymous()
            .WithTags(nameof(UtilityBills));
    }
}
