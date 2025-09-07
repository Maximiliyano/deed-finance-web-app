using Deed.Domain.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;

namespace Deed.Application.Abstractions.Behaviours;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        Log.Information("Processing request {RequestName}", requestName);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            Log.Information("Completed request {RequestName}", requestName);
        }
        else
        {
            using (LogContext.PushProperty(nameof(result.Errors), result.Errors, true))
            {
                Log.Error("Completed request {RequestName} with errors", requestName);
            }
        }

        return result;
    }
}
