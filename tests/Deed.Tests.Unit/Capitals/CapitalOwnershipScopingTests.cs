using Deed.Application.Auth;
using Deed.Application.Debts.Commands.Create;
using Deed.Application.Debts.Commands.Update;
using Deed.Application.Expenses.Commands.Create;
using Deed.Application.Incomes.Commands.Create;
using Deed.Application.Transfers.Commands.Create;
using Deed.Application.Transfers.Commands.Delete;
using Deed.Application.Transfers.Specifications;
using Deed.Application.UtilityBills.Commands.Pay;
using Deed.Application.UtilityBills.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals;

public sealed class CapitalOwnershipScopingTests
{
    private const string TestUser = "testuser";

    private readonly IUser _user = Substitute.For<IUser>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly List<ISpecification<Capital>> _capitalSpecs = [];

    public CapitalOwnershipScopingTests()
    {
        _user.Name.Returns(TestUser);
        _user.IsAuthenticated.Returns(true);

        _capitalRepository
            .GetAsync(Arg.Do<ISpecification<Capital>>(s => _capitalSpecs.Add(s)), Arg.Any<CancellationToken>())
            .Returns(NewCapital(1, TestUser));
    }

    [Fact]
    public async Task CreateExpense_LooksUpCapitalScopedToCurrentUser()
    {
        // Arrange
        var handler = new CreateExpenseCommandHandler(
            _user, _capitalRepository, Substitute.For<IExpenseRepository>(), Substitute.For<ITagRepository>(), _unitOfWork);

        // Act
        await handler.Handle(new CreateExpenseCommand(7, 1, 10m, DateTimeOffset.UtcNow, null, []), CancellationToken.None);

        // Assert
        AssertScopedToTestUser(7);
    }

    [Fact]
    public async Task CreateIncome_LooksUpCapitalScopedToCurrentUser()
    {
        // Arrange
        var handler = new CreateIncomeCommandHandler(
            _user, _capitalRepository, Substitute.For<IIncomeRepository>(), Substitute.For<ITagRepository>(), _unitOfWork);

        // Act
        await handler.Handle(new CreateIncomeCommand(7, 1, 10m, DateTimeOffset.UtcNow, null, []), CancellationToken.None);

        // Assert
        AssertScopedToTestUser(7);
    }

    [Fact]
    public async Task CreateTransfer_LooksUpBothCapitalsScopedToCurrentUser()
    {
        // Arrange
        var handler = new CreateTransferCommandHandler(
            _user, _capitalRepository, Substitute.For<ITransferRepository>(), _unitOfWork);

        // Act
        await handler.Handle(new CreateTransferCommand(3, 4, 10m, 10m), CancellationToken.None);

        // Assert
        _capitalSpecs.Should().HaveCount(2);
        AssertScopedToTestUser(3, _capitalSpecs[0]);
        AssertScopedToTestUser(4, _capitalSpecs[1]);
    }

    [Fact]
    public async Task DeleteTransfer_LooksUpBothCapitalsScopedToCurrentUser()
    {
        // Arrange
        var transferRepository = Substitute.For<ITransferRepository>();
        transferRepository.GetAsync(Arg.Any<TransferByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new Transfer { Amount = 10m, SourceCapitalId = 3, DestinationCapitalId = 4 });

        var handler = new DeleteTransferCommandHandler(transferRepository, _capitalRepository, _unitOfWork, _user);

        // Act
        await handler.Handle(new DeleteTransferCommand(1), CancellationToken.None);

        // Assert
        _capitalSpecs.Should().HaveCount(2);
        AssertScopedToTestUser(3, _capitalSpecs[0]);
        AssertScopedToTestUser(4, _capitalSpecs[1]);
    }

    [Fact]
    public async Task CreateDebt_LooksUpCapitalScopedToCurrentUser()
    {
        // Arrange
        var handler = new CreateDebtCommandHandler(_user, Substitute.For<IDebtRepository>(), _capitalRepository, _unitOfWork);
        var command = new CreateDebtCommand("Laptop", 100m, nameof(CurrencyType.USD), "A", "B", DateTimeOffset.UtcNow, null, null, 7);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        AssertScopedToTestUser(7);
    }

    [Fact]
    public async Task UpdateDebt_LooksUpPayFromCapitalScopedToCurrentUser()
    {
        // Arrange
        var debtRepository = Substitute.For<IDebtRepository>();
        debtRepository.GetAsync(Arg.Any<DebtByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new Debt(1)
            {
                Item = "Laptop",
                Amount = 100m,
                Currency = CurrencyType.USD,
                Source = "A",
                Recipient = "B",
                BorrowedAt = DateTimeOffset.UtcNow
            });

        var handler = new UpdateDebtCommandHandler(debtRepository, _capitalRepository, _unitOfWork, _user);

        // Act
        await handler.Handle(new UpdateDebtCommand(1, IsPaid: true, PayFromCapitalId: 7), CancellationToken.None);

        // Assert
        AssertScopedToTestUser(7);
    }

    [Fact]
    public async Task PayUtilityBill_LooksUpCapitalScopedToCurrentUser()
    {
        // Arrange
        var billRepository = Substitute.For<IUtilityBillRepository>();
        billRepository.GetAsync(Arg.Any<UtilityBillByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new UtilityBill(1)
            {
                Name = "Electric",
                EstimatedAmount = 50m,
                Currency = CurrencyType.UAH,
                DueDayOfMonth = 10,
                CapitalId = 7,
                CategoryId = 3
            });

        var handler = new PayUtilityBillCommandHandler(
            billRepository, _capitalRepository, Substitute.For<IExpenseRepository>(), _unitOfWork, _user);

        // Act
        await handler.Handle(new PayUtilityBillCommand(1, 50m, null), CancellationToken.None);

        // Assert
        AssertScopedToTestUser(7);
    }

    private void AssertScopedToTestUser(int capitalId) =>
        AssertScopedToTestUser(capitalId, _capitalSpecs.Should().ContainSingle().Subject);

    private static void AssertScopedToTestUser(int capitalId, ISpecification<Capital> specification)
    {
        var matches = specification.Criteria!.Compile();

        matches(NewCapital(capitalId, TestUser)).Should().BeTrue();
        matches(NewCapital(capitalId, "other-user")).Should().BeFalse();
    }

    private static Capital NewCapital(int id, string createdBy) =>
        new(id) { Name = "Wallet", Balance = 100m, Currency = CurrencyType.USD, CreatedBy = createdBy };
}
