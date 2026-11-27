namespace Deed.Domain.Results;

public enum ErrorType
{
    Failure,
    Validation,
    BadRequest,
    NotFound,
    Conflict,
    TooManyRequests,
}
