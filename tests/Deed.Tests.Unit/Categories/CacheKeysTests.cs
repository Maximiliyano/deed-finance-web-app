using Deed.Application.Abstractions.Caching;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.Categories;

public sealed class CacheKeysTests
{
    [Fact]
    public void AllCategoryKeys_CoversEveryTypeAndIncludeDeletedCombination()
    {
        // Arrange
        CategoryType?[] types = [null, CategoryType.None, CategoryType.Expenses, CategoryType.Incomes];
        bool?[] includeDeleted = [null, true, false];

        // Act
        var keys = CacheKeys.AllCategoryKeys().ToList();

        // Assert
        keys.Should().OnlyHaveUniqueItems().And.HaveCount(types.Length * includeDeleted.Length);
        foreach (var type in types)
        {
            foreach (var flag in includeDeleted)
            {
                keys.Should().Contain(CacheKeys.Category(type, flag));
            }
        }
    }
}
