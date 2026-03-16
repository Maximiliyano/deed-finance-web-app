using Deed.Application.Capitals.Specifications;
using Deed.Application.Incomes.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Commands;

public sealed class CreateIncomeCommandHandlerTests
{
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IIncomeRepository _incomeRepositoryMock = Substitute.For<IIncomeRepository>();
    private readonly ITagRepository _tagRepositoryMock = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateIncomeCommandHandler _handler;

    public CreateIncomeCommandHandlerTests()
    {
        _handler = new CreateIncomeCommandHandler(_capitalRepositoryMock, _incomeRepositoryMock, _tagRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnId_WhenIncomeCreated()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 100,
            Currency = CurrencyType.UAH
        };

        var command = new CreateIncomeCommand(capital.Id, 1, 100m, DateTimeOffset.UtcNow, null, []);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
            .Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        capital.Balance.Should().Be(200m);

        _capitalRepositoryMock.Received(1).Update(capital);
        _incomeRepositoryMock.Received(1).Create(Arg.Is<Income>(i => i.Amount.Equals(command.Amount)));

        await _capitalRepositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCapitalNotFound()
    {
        // Arrange
        var command = new CreateIncomeCommand(1, 1, 100m, DateTimeOffset.UtcNow, null, []);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>())
            .Returns((Capital?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("capital"));

        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        _incomeRepositoryMock.DidNotReceive().Create(Arg.Any<Income>());

        await _capitalRepositoryMock.Received(1).GetAsync(Arg.Any<CapitalByIdSpecification>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreateTags_WhenTagNamesProvided()
    {
        // Arrange
        var capital = new Capital(1)
        {
            Name = "TestCapital",
            Balance = 500,
            Currency = CurrencyType.USD
        };

        var command = new CreateIncomeCommand(capital.Id, 1, 200m, DateTimeOffset.UtcNow, null, ["salary", "bonus"]);

        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _tagRepositoryMock.GetAsync(Arg.Any<ISpecification<Tag>>()).Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepositoryMock.Received(2).Create(Arg.Any<Tag>());
    }
}
