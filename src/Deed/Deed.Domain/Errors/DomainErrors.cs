using Deed.Domain.Results;

namespace Deed.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static Error Exception
            => Error.Failure(nameof(Exception), "An unexcepted error occured.");

        public static Error NotFound(string name)
            => Error.NotFound(nameof(NotFound), $"The specific {name} was not found.");
    }

    public static class Capital
    {
        public static Error InvalidCurrency
            => Error.BadRequest(nameof(InvalidCurrency), "The currency is invalid.");
    }

    public static class Exchange
    {
        public static Error HttpExecution
            => Error.Failure(nameof(HttpExecution), "The http request execution was failed.");

        public static Error Serialization
            => Error.Failure(nameof(Serialization), "The content execution into exchange was failed.");

        public static Error AlreadyExists
            => Error.Conflict(nameof(AlreadyExists), "The exchange already exists.");

        public static Error InvalidOperation
            => Error.BadRequest(nameof(InvalidOperation), "The operation is invalid.");
    }

    public static class Category
    {
        public static Error InvalidPerPeriod
            => Error.BadRequest(nameof(InvalidPerPeriod), "Category per period is invalid.");
    }
}
