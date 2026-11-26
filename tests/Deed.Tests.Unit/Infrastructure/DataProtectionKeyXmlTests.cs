using Deed.Infrastructure.Services;
using FluentAssertions;

namespace Deed.Tests.Unit.Infrastructure;

public sealed class DataProtectionKeyXmlTests
{
    private const string Id = "11111111-2222-3333-4444-555555555555";

    private const string Plaintext = $"""
        <key id="{Id}" version="1"><creationDate>2026-01-01T00:00:00Z</creationDate><descriptor deserializerType="x"><descriptor><encryption algorithm="AES_256_CBC" /><validation algorithm="HMACSHA256" /><masterKey p4:requiresEncryption="true" xmlns:p4="http://schemas.asp.net/2015/03/dataProtection"><!-- Warning: the key below is in an unencrypted form. --><value>YWJj</value></masterKey></descriptor></descriptor></key>
        """;

    private const string Encrypted = $"""
        <key id="{Id}" version="1"><creationDate>2026-01-01T00:00:00Z</creationDate><descriptor deserializerType="x"><descriptor><encryption algorithm="AES_256_CBC" /><validation algorithm="HMACSHA256" /><enc:encryptedSecret decryptorType="y" xmlns:enc="http://schemas.asp.net/2015/03/dataProtection"><EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element" xmlns="http://www.w3.org/2001/04/xmlenc#" /></enc:encryptedSecret></descriptor></descriptor></key>
        """;

    [Fact]
    public void IsPlaintextKey_KeyWithMasterKeyValue_ReturnsTrue()
    {
        // Arrange & Act
        var result = DataProtectionKeyXml.IsPlaintextKey(Plaintext);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPlaintextKey_KeyProtectedByCertificate_ReturnsFalse()
    {
        // Arrange & Act
        var result = DataProtectionKeyXml.IsPlaintextKey(Encrypted);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not xml")]
    [InlineData("<revocation version=\"1\"><revocationDate>2026-01-01T00:00:00Z</revocationDate></revocation>")]
    public void IsPlaintextKey_NotAKey_ReturnsFalse(string? xml)
    {
        // Arrange & Act
        var result = DataProtectionKeyXml.IsPlaintextKey(xml);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void KeyId_KeyElement_ReturnsItsId()
    {
        // Arrange & Act
        var result = DataProtectionKeyXml.KeyId(Plaintext);

        // Assert
        result.Should().Be(Guid.Parse(Id));
    }

    [Fact]
    public void KeyId_InvalidXml_ReturnsNull()
    {
        // Arrange & Act
        var result = DataProtectionKeyXml.KeyId("not xml");

        // Assert
        result.Should().BeNull();
    }
}
