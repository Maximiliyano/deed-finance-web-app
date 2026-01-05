using Deed.Api.Extensions;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Deed.Api.Middlewares;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        else
        {
            Log.Error(exception, "Unhandled exception occured");

            var error = ParseException(exception);
            var statusCode = error.Type.GetStatusCode();

            httpContext.Response.StatusCode = statusCode;

            await problemDetailsService.WriteAsync(new()
            {
                HttpContext = httpContext,
                Exception = exception,
            });
        }

        return true;
    }

    private static Error ParseException(Exception exception)
        => exception switch
        {
            _ => DomainErrors.General.Exception
        };
}
