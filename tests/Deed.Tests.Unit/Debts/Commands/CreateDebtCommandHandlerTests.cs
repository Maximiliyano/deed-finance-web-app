using Deed.Application.Debts.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Debts.Commands;

public sealed class CreateDebtCommandHandlerTests
{
    private readonly IDebtRepository _debtRepository = Substitute.For<IDebtRepository>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateDebtCommandHandler _handler;

    public CreateDebtCommandHandlerTests()
    {
        _handler = new CreateDebtCommandHandler(_debtRepository, _capitalRepository, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidDebt_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateDebtCommand(
            "Laptop  ", 1500m, CurrencyType.USD,
            "John  ", "Me  ", DateTimeOffset.UtcNow,
            null, "  Some note  ", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _debtRepository.Received(1).Create(Arg.Is<Debt>(d =>
            d.Item == "Laptop" &&
            d.Amount == 1500m &&
            d.Currency == CurrencyType.USD &&
            d.Source == "John" &&
            d.Recipient == "Me" &&
            d.Note == "Some note"
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateDebtWithNullNote_SetsNoteToNull()
    {
        // Arrange
        var command = new CreateDebtCommand(
            "Rent", 500m, CurrencyType.UAH,
            "Bank", "Me", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(1), null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _debtRepository.Received(1).Create(Arg.Is<Debt>(d =>
            d.Note == null &&
            d.DeadlineAt != null
        ));
    }
}
