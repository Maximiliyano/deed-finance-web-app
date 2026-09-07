using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deed.Tests.Integration.Infrastructure;

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims = new[]
        {
            new Claim("name", DeedApiFactory.TestUser),
            new Claim(ClaimTypes.NameIdentifier, DeedApiFactory.TestUser),
            new Claim(ClaimTypes.Email, "test@deed.finance"),
            new Claim("email_verified", "true")
        };

        ClaimsIdentity identity = new(claims, "Test");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
