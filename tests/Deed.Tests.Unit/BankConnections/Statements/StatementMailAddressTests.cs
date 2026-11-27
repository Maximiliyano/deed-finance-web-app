using Deed.Application.BankConnections.Statements;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Statements;

public sealed class StatementMailAddressTests
{
    private const string Base = "deed.statements@gmail.com";
    private const string Key = "A1B2C3D4E5F60718";

    [Fact]
    public void NewKey_ReturnsSixteenUppercaseHexCharacters()
    {
        // Arrange
        // Act
        var key = StatementMailAddress.NewKey();

        // Assert
        key.Should().MatchRegex("^[0-9A-F]{16}$");
    }

    [Fact]
    public void Template_InsertsThePlaceholderAfterThePlus()
    {
        // Arrange
        // Act
        var template = StatementMailAddress.Template(Base);

        // Assert
        template.Should().Be("deed.statements+{key}@gmail.com");
    }

    [Fact]
    public void Template_AddressAlreadyWithPlus_ReplacesTheTag()
    {
        // Arrange
        // Act
        var template = StatementMailAddress.Template("deed+old@gmail.com");

        // Assert
        template.Should().Be("deed+{key}@gmail.com");
    }

    [Fact]
    public void ExtractKeys_ToHeaderWithKey_ReturnsTheKey()
    {
        // Arrange
        var addresses = new[] { $"deed.statements+{Key.ToUpperInvariant()}@gmail.com", "someone@else.com" };

        // Act
        var keys = StatementMailAddress.ExtractKeys(addresses, Base);

        // Assert
        keys.Should().Equal(Key);
    }

    [Fact]
    public void ExtractKeys_IsCaseInsensitiveAndTrimsBrackets()
    {
        // Arrange
        var addresses = new[] { $"<Deed.Statements+{Key}@GMAIL.com>" };

        // Act
        var keys = StatementMailAddress.ExtractKeys(addresses, Base);

        // Assert
        keys.Should().Equal(Key);
    }

    [Theory]
    [InlineData("deed.statements@gmail.com")]
    [InlineData("deed.statements+short@gmail.com")]
    [InlineData("deed.statements+A1B2C3D4E5F60718@evil.com")]
    [InlineData("other+A1B2C3D4E5F60718@gmail.com")]
    [InlineData("deed.statements+ZZZZZZZZZZZZZZZZ@gmail.com")]
    public void ExtractKeys_AddressWithoutAValidKey_ReturnsNothing(string address)
    {
        // Arrange
        // Act
        var keys = StatementMailAddress.ExtractKeys([address], Base);

        // Assert
        keys.Should().BeEmpty();
    }

    [Fact]
    public void ExtractKeys_RepeatedKey_IsReturnedOnce()
    {
        // Arrange
        var address = $"deed.statements+{Key}@gmail.com";

        // Act
        var keys = StatementMailAddress.ExtractKeys([address, address, null], Base);

        // Assert
        keys.Should().Equal(Key);
    }
}
