using Deed.Domain.Enums;

namespace Deed.Application.Abstractions.Caching;

public static class CacheKeys
{
    public const string Exchanges = "deed:exchanges";
    public const string Categories = "deed:categories";

    private static readonly CategoryType?[] CategoryTypes = [null, .. Enum.GetValues<CategoryType>().Cast<CategoryType?>()];
    private static readonly bool?[] IncludeDeletedValues = [null, true, false];

    public static string Category(CategoryType? type, bool? includeDeleted) => $"{Categories}:{type}:{includeDeleted}";

    public static IEnumerable<string> AllCategoryKeys() =>
        from type in CategoryTypes
        from includeDeleted in IncludeDeletedValues
        select Category(type, includeDeleted);
}
