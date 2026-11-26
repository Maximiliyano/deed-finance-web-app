using Deed.Domain.Results;

namespace Deed.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static Error Exception
            => Error.Failure(nameof(Exception), "An unexpected error occurred.");

        public static Error NotFound(string name)
            => Error.NotFound(nameof(NotFound), $"The specified {name} was not found.");

        public static Error ConcurrencyConflict
            => Error.Conflict(nameof(ConcurrencyConflict), "The data was changed by another request. Reload and try again.");

        public static Error UpdateFailed
            => Error.Failure(nameof(UpdateFailed), "Update entity failed.");

        public static Error EmptyCollection
            => Error.BadRequest(nameof(EmptyCollection), "The collection of entities is empty.");
        
        public static Error InvalidCurrency
            => Error.BadRequest(nameof(InvalidCurrency), "The currency is invalid.");
    }

    public static class Capital
    {
        public static Error ForSavingsOnly
            => Error.BadRequest(nameof(ForSavingsOnly), "Capital is only for savings.");

        public static Error ReferenceExists
            => Error.Conflict("Capital.ReferenceExists", "Capital cannot be deleted because it has related references.");
    }

    public static class HttpClient
    {
        public static Error Execution
            => Error.Failure(nameof(Execution), "The http request execution was failed.");

        public static Error Serialization
            => Error.Failure(nameof(Serialization), "The content execution into exchange was failed.");

        public static Error InvalidOperation
            => Error.BadRequest(nameof(InvalidOperation), "The operation is invalid.");
    }

    public static class Bank
    {
        public static Error TokenInvalid
            => Error.BadRequest("Bank.TokenInvalid", "The bank token is invalid or has expired.");

        public static Error NotConnected
            => Error.NotFound("Bank.NotConnected", "No bank account is connected.");

        public static Error RateLimited
            => Error.BadRequest("Bank.RateLimited", "The bank allows one request per minute. Try again shortly.");

        public static Error CapitalNotLinked
            => Error.BadRequest("Bank.CapitalNotLinked", "Link a capital to this bank account first.");

        public static Error CurrencyMismatch
            => Error.BadRequest("Bank.CurrencyMismatch", "The transaction currency differs from the linked capital currency.");

        public static Error CategoryRequired
            => Error.BadRequest("Bank.CategoryRequired", "Choose a category for this transaction.");

        public static Error CategoryTypeMismatch
            => Error.BadRequest("Bank.CategoryTypeMismatch", "The category does not match the transaction direction.");

        public static Error InvalidPeriod
            => Error.BadRequest("Bank.InvalidPeriod", "The start of the period must be before its end.");

        public static Error AccountNotFound
            => Error.NotFound("Bank.AccountNotFound", "This bank account was not found. Refresh your accounts and try again.");

        public static Error AccountAlreadyConnected
            => Error.Conflict("Bank.AccountAlreadyConnected", "This bank account is already connected.");

        public static Error NotPending
            => Error.Conflict("Bank.NotPending", "This transaction is no longer waiting for confirmation.");
    }

    public static class Category
    {
        public static Error PeriodAmountGreaterEqualZero
            => Error.BadRequest(nameof(PeriodAmountGreaterEqualZero), "Period amount should be greater or equal zero.");

        public static Error ReferenceExists
            => Error.Conflict("Category.ReferenceExists", "Category cannot be deleted because it has related references.");
    }

    public static class Anonymous
    {
        public static Error LimitReached
            => Error.BadRequest("Anonymous.LimitReached", "Anonymous users can create up to 3 items per entity. Sign in to remove this limit.");

        public static Error NameInvalid
            => Error.Failure("Anonymous.NameInvalid", "Server failure. Anonymous user name is invalid.");
    }
}
