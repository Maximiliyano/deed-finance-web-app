using Deed.Domain.Results;

namespace Deed.Api.Extensions;

internal static class ResultExtensions
{
    internal static IResult Process(this Result result, ResultType type = ResultType.Ok)
    {
        return result.IsSuccess
            ? ToResult(type)
            : ToProblemDetails(result.Errors);
    }

    internal static IResult Process<TValue>(this Result<TValue> result, ResultType type = ResultType.Ok)
    {
        return result.IsSuccess
            ? ToResult(type, result.Value)
            : ToProblemDetails(result.Errors);
    }

    private static IResult ToResult(ResultType type, object? value = null)
        => type switch
        {
            ResultType.Ok => Results.Ok(value),
            ResultType.NoContent => Results.NoContent(),
            _ => throw new ArgumentException(ResultConstants.InvalidResultTypeMessage)
        };

    internal static IResult ToProblemDetails(IList<Error> errors)
    {
        var error = errors[0];

        return Results.Problem(
            statusCode: error.Type.GetStatusCode(),
            title: error.Code,
            type: error.Type.ToString(),
            extensions: new Dictionary<string, object?>
            {
                { nameof(errors), errors }
            });
    }
}
