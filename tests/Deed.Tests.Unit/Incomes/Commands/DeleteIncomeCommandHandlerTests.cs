using Deed.Application.Auth;
using Deed.Application.Incomes.Commands.Delete;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Commands;

public sealed class DeleteIncomeCommandHandlerTests
{
    private readonly IIncomeRepository _incomeRepository = Substitute.For<IIncomeRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteIncomeCommandHandler _handler;

    public DeleteIncomeCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DeleteIncomeCommandHandler(_incomeRepository, _capitalRepository, _unitOfWork, _userMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnNoContent_WhenIncomeDeleted()
    {
        // Arrange
        var income = new Income(1)
        {
            Amount = 10,
            PaymentDate = DateTimeOffset.UtcNow,
            CategoryId = 1,
            Category = new Category(1)
            {
                Name = "TestCategory",
                Type = CategoryType.Incomes
            },
            CapitalId = 1,
            Capital = new Capital(1)
            {
                Name = "TestCapital",
                Balance = 100,
                Currency = CurrencyType.EUR
            }
        };

        var command = new DeleteIncomeCommand(income.Id);

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns(income);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        income.Capital.Balance.Should().Be(90);

        _capitalRepository.Received(1).Update(income.Capital);
        _incomeRepository.Received(1).Delete(income);

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenIncomeNotFound()
    {
        // Arrange
        var command = new DeleteIncomeCommand(1);

        _incomeRepository.GetAsync(Arg.Any<IncomeByIdSpecification>())
            .Returns((Income)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Arrange
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("income"));

        _capitalRepository.DidNotReceive().Update(Arg.Any<Capital>());
        _incomeRepository.DidNotReceive().Delete(Arg.Any<Income>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
