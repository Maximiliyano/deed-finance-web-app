using Deed.Domain.Results;
using FluentValidation;
using MediatR;

namespace Deed.Application.Abstractions.Behaviours;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) :
    IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(request, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(
                failure.ErrorCode,
                failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
        {
            return CreateResult<TResponse>(errors);
        }

        return await next(cancellationToken);
    }

    private static TResult CreateResult<TResult>(IList<Error> errors)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(errors) as TResult)!;
        }

        var result = typeof(Result<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(Result.Failure))!
            .Invoke(null, [errors]);

        return (TResult)result!;
    }
}
