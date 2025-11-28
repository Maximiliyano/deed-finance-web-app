namespace Deed.Domain.Results;

public class Result
{
    protected Result(bool isSuccess, IList<Error> errors)
    {
        if (isSuccess && errors.Any(x => x != Error.None) ||
            !isSuccess && errors.Any(x => x == Error.None))
        {
            throw new ArgumentException(ResultConstants.InvalidError, nameof(errors));
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public IList<Error> Errors { get; protected init; }

    public static Result Success() => new(true, [Error.None]);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, [Error.None]);

    public static Result Failure(Error error) => Failure([error]);

    public static Result Failure(IList<Error> errors) => new(false, errors);

    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, [error]);

    public static Result<TValue> Failure<TValue>(IList<Error> errors) => new(default!, false, errors);
}

public class Result<TValue> : Result
{
    protected internal Result(TValue value, bool isSuccess, IList<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static new Result<TValue> Failure(IList<Error> errors) => new(default!, false, errors);

    public TValue Value { get; }
}
