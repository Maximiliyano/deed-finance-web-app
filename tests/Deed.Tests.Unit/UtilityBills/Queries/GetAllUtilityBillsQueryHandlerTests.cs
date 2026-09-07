using Deed.Application.Auth;
using Deed.Application.UtilityBills.Queries.GetAll;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Queries;

public sealed class GetAllUtilityBillsQueryHandlerTests
{
    private readonly IUtilityBillRepository _repositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetAllUtilityBillsQueryHandler _handler;

    public GetAllUtilityBillsQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetAllUtilityBillsQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ReturnsBillsMappedToResponses()
    {
        // Arrange
        var bills = new List<UtilityBill>
        {
            new(1)
            {
                Name = "Electric",
                EstimatedAmount = 500m,
                Currency = CurrencyType.UAH,
                DueDayOfMonth = 15,
                CapitalId = 1,
                CategoryId = 7,
                IsActive = true
            },
            new(2)
            {
                Name = "Water",
                EstimatedAmount = 200m,
                Currency = CurrencyType.UAH,
                DueDayOfMonth = 10,
                CapitalId = 1,
                CategoryId = 7,
                IsActive = false
            }
        };

        _repositoryMock
            .GetAllAsync(Arg.Any<UtilityBillsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(bills);

        // Act
        var result = await _handler.Handle(new GetAllUtilityBillsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var responses = result.Value.ToList();
        responses.Should().HaveCount(2);
        responses[0].Name.Should().Be("Electric");
        responses[0].IsActive.Should().BeTrue();
        responses[0].IsPaidThisMonth.Should().BeFalse();
        responses[1].Name.Should().Be("Water");
        responses[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenBillPaidThisMonth_ReportsPaidStatus()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var bill = new UtilityBill(1)
        {
            Name = "Internet",
            EstimatedAmount = 300m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 20,
            CapitalId = 1,
            CategoryId = 7,
            IsActive = true,
        };
        bill.Payments.Add(new Expense
        {
            Amount = 295m,
            CategoryId = 7,
            CapitalId = 1,
            PaymentDate = now,
            UtilityBillId = 1
        });

        _repositoryMock
            .GetAllAsync(Arg.Any<UtilityBillsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new[] { bill });

        // Act
        var result = await _handler.Handle(new GetAllUtilityBillsQuery(), CancellationToken.None);

        // Assert
        var response = result.Value.Single();
        response.IsPaidThisMonth.Should().BeTrue();
        response.PaidAmount.Should().Be(295m);
    }
}
