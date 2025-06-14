﻿using Deed.Api.Extensions;
using Deed.Application.Capitals.Queries.GetById;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/capitals/{id:int}", async (int id, ISender sender) =>
            (await sender
                .Send(new GetByIdCapitalQuery(id)))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
