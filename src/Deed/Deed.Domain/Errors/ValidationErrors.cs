using Deed.Domain.Constants;
using Deed.Domain.Results;

namespace Deed.Domain.Errors;

public static class ValidationErrors
{
    public static class Expense
    {
        public static Error InvalidPaymentDate
            => Error.BadRequest(nameof(InvalidPaymentDate), "The expense payment date was invalid.");
    }

    public static class Capital
    {
        public static Error AlreadyExists
            => Error.Conflict(nameof(AlreadyExists), "The capital was already exists.");

        public static Error InvalidCurrencyType
            => Error.BadRequest(nameof(InvalidCurrencyType), "The capital currency type is invalid.");
    }

    public static class Category
    {
        public static Error EmptyName
            => Error.BadRequest(nameof(EmptyName), "The category name is required.");

        public static Error NameTooLong
            => Error.BadRequest(nameof(NameTooLong), $"The category name should be less than {ValidationConstants.MaxLenghtName}");

        public static Error InvalidType
            => Error.BadRequest(nameof(InvalidType), "The category type is invalid.");

        public static Error AlreadyExists
            => Error.Conflict(nameof(AlreadyExists), "The category was already exists.");
    }

    public static class General
    {
        public static Error NotFound(string entity)
            => Error.NotFound(nameof(NotFound), $"The {entity} was not found.");

        public static Error NameAlreadyExists
            => Error.Conflict(nameof(NameAlreadyExists), "The entity name was already exists.");

        public static Error AmountMustBeGreaterThanZero
            => Error.BadRequest(nameof(AmountMustBeGreaterThanZero), "Amount must be greater than zero.");
    }
}
