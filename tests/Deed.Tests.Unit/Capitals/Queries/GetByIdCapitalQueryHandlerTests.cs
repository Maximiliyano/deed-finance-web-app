using Deed.Application.Auth;
using Deed.Application.Capitals.Queries.GetById;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Queries;

public sealed class GetByIdCapitalQueryHandlerTests
{
    private readonly ICapitalRepository _repositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetByIdCapitalQueryHandler _handler;

    public GetByIdCapitalQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetByIdCapitalQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnCapital_WhenCapitalExists()
    {
        // Arrange
        const int id = 1;

        var capital = new Capital(id) { Name = "New Capital", Balance = 100, Currency = CurrencyType.UAH };
        var query = new GetByIdCapitalQuery(id);

        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be(capital.Name);
        result.Value.Balance.Should().Be(capital.Balance);
        result.Value.Currency.Should().Be(capital.Currency.ToString());

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCapitalNotFound()
    {
        // Arrange
        _repositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
                       .Returns((Capital)null);

        var query = new GetByIdCapitalQuery(-1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("capital"));

        await _repositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
    }
}
