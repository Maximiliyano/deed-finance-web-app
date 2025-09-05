using Deed.Api.Extensions;
using Deed.Application.Expenses.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Expenses;

internal sealed class GetAllGroupByCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenses", async (ISender sender, int? capitalId) =>
            (await sender
                .Send(new GetExpensesByCategoryQuery(capitalId)))
                .Process())
            .WithTags(nameof(Expenses));
    }
}
