using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Statements;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Statements;

public sealed class StatementMailPolicyTests
{
    private const string Passing = "mx.google.com; dkim=pass header.i=@privatbank.ua header.s=sel header.b=abc; spf=pass (google.com: domain of noreply@privatbank.ua designates 1.2.3.4 as permitted sender) smtp.mailfrom=noreply@privatbank.ua";

    private readonly StatementMailSettings _settings = new();

    [Fact]
    public void IsTrusted_AllowedSenderWithDkimPass_IsTrusted()
    {
        // Arrange
        // Act
        var result = StatementMailPolicy.IsTrusted("noreply@privatbank.ua", Passing, _settings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTrusted_SubdomainOfAllowedSender_IsTrusted()
    {
        // Arrange
        // Act
        var result = StatementMailPolicy.IsTrusted("statements@mail.privatbank.ua", Passing, _settings);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("noreply@evil.com")]
    [InlineData("noreply@privatbank.ua.evil.com")]
    [InlineData("noreply@notprivatbank.ua")]
    [InlineData(null)]
    public void IsTrusted_UnknownSender_IsRejected(string? from)
    {
        // Arrange
        // Act
        var result = StatementMailPolicy.IsTrusted(from, Passing, _settings);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("mx.google.com; dkim=fail header.i=@privatbank.ua; spf=fail smtp.mailfrom=noreply@privatbank.ua")]
    [InlineData("mx.google.com; dkim=pass header.i=@evil.com; spf=pass smtp.mailfrom=bounce@evil.com")]
    public void IsTrusted_MissingOrFailingAuthentication_IsRejected(string? header)
    {
        // Arrange
        // Act
        var result = StatementMailPolicy.IsTrusted("noreply@privatbank.ua", header, _settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_AuthenticationNotRequired_OnlyChecksTheSender()
    {
        // Arrange
        var settings = new StatementMailSettings { RequireAuthenticationResults = false };

        // Act
        var result = StatementMailPolicy.IsTrusted("noreply@privatbank.ua", null, settings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTrusted_TrustedAuthServIdMismatch_IsRejected()
    {
        // Arrange
        var settings = new StatementMailSettings { TrustedAuthServId = "mx.example.org" };

        // Act
        var result = StatementMailPolicy.IsTrusted("noreply@privatbank.ua", Passing, settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_TrustedAuthServIdMatches_IsTrusted()
    {
        // Arrange
        var settings = new StatementMailSettings { TrustedAuthServId = "MX.GOOGLE.COM" };

        // Act
        var result = StatementMailPolicy.IsTrusted("noreply@privatbank.ua", Passing, settings);

        // Assert
        result.Should().BeTrue();
    }
}
