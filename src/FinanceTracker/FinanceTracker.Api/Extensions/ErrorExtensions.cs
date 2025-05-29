using FinanceTracker.Domain.Results;

namespace FinanceTracker.Api.Extensions;

internal static class ErrorExtensions
{
    internal static int GetStatusCode(this ErrorType type)
        => type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
}
