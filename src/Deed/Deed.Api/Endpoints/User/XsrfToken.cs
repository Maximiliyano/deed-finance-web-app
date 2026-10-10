using Microsoft.AspNetCore.Antiforgery;

namespace Deed.Api.Endpoints.User;

internal sealed class XsrfToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/xsrf", (IAntiforgery antiforgery, HttpContext ctx) =>
        {
            antiforgery.GetAndStoreTokens(ctx);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithTags(nameof(User));
    }
}
