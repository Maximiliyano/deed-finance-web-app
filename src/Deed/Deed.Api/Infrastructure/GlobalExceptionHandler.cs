using Deed.Api.Extensions;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Deed.Api.Infrastructure;

internal sealed class GlobalExceptionHandler : IExceptionHandler
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

            var errors = ParseException(exception);
        
            var error = errors[0];
            var statusCode = error.Type.GetStatusCode();
            var problemDetails = BuildProblemDetails(statusCode, error.Message, errors);

            httpContext.Response.StatusCode = problemDetails.Status!.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }

    private static ProblemDetails BuildProblemDetails(int statusCode, string title, Error[] errors)
        => new()
        {
            Type = nameof(ProblemDetails),
            Title = title,
            Status = statusCode,
            Extensions = new Dictionary<string, object?>
            {
                { nameof(errors), errors }
            }
        };

    private static Error[] ParseException(Exception exception)
        => exception switch
        {
            _ => [DomainErrors.General.Exception]
        };
}
