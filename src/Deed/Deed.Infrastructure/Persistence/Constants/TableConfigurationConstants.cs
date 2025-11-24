using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Constants;

internal static class TableConfigurationConstants
{
    public const string Capitals = nameof(Capitals);

    public const string Expenses = nameof(Expenses);

    public const string Exchanges = nameof(Exchanges);

    public const string Incomes = nameof(Incomes);

    public const string Transfers = nameof(Transfers);

    public const string Categories = nameof(Categories);

    public const string DatabaseConnection = nameof(DatabaseConnection);

    public const string Users = nameof(Users);
}
