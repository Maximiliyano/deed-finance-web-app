using Deed.Application.Auth;
using Deed.Application.Transfers.Queries.GetAll;
using Deed.Application.Transfers.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Transfers.Queries;

public sealed class GetAllTransfersQueryHandlerTests
{
    private readonly ITransferRepository _repository = Substitute.For<ITransferRepository>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly GetAllTransfersQueryHandler _handler;

    public GetAllTransfersQueryHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new GetAllTransfersQueryHandler(_repository, _user);
    }

    [Fact]
    public async Task Handle_ReturnsTransferList()
    {
        // Arrange
        List<Transfer> transfers = new()
        {
            new Transfer
            {
                Amount = 100m, DestinationAmount = 100m,
                SourceCapitalId = 1, DestinationCapitalId = 2,
                SourceCapital = new Capital { Name = "Cash", Balance = 0, Currency = CurrencyType.UAH },
                DestinationCapital = new Capital { Name = "Bank", Balance = 0, Currency = CurrencyType.USD }
            }
        };
        _repository.GetAllAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(transfers.AsEnumerable());

        // Act
        Result<IEnumerable<TransferResponse>> result =
            await _handler.Handle(new GetAllTransfersQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Transfer>());

        // Act
        Result<IEnumerable<TransferResponse>> result =
            await _handler.Handle(new GetAllTransfersQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
