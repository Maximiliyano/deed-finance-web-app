using Deed.Application.BankConnections.Queries.GetPrivatAccounts;
using FluentValidation.TestHelper;

namespace Deed.Tests.Unit.BankConnections.Queries;

public sealed class GetPrivatAccountsQueryValidatorTests
{
    private readonly GetPrivatAccountsQueryValidator _validator = new();

    [Fact]
    public void Validate_Token_PassesValidation()
    {
        // Arrange
        var query = new GetPrivatAccountsQuery("token");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyToken_FailsValidation(string? token)
    {
        // Arrange
        var query = new GetPrivatAccountsQuery(token!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.Token);
    }

    [Fact]
    public void Validate_TooLongToken_FailsValidation()
    {
        // Arrange
        var query = new GetPrivatAccountsQuery(new string('a', 513));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.Token);
    }
}
