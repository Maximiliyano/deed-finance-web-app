using Deed.Application.Auth;
using Deed.Application.Incomes;
using Deed.Application.Incomes.Queries.GetAll;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Incomes.Queries;

public sealed class GetAllIncomesQueryHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IIncomeRepository _incomeRepositoryMock = Substitute.For<IIncomeRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly ICategoryRepository _categoryRepositoryMock = Substitute.For<ICategoryRepository>();

    private readonly GetIncomesQueryHandler _handler;

    public GetAllIncomesQueryHandlerTests()
    {
        _handler = new GetIncomesQueryHandler(_userMock, _incomeRepositoryMock, _categoryRepositoryMock, _capitalRepositoryMock);
    }

    [Fact]
    public async Task Handle_GetAllExpenses_ShouldReturnAll()
    {
        // Arrange
        var entity = new Income(1)
        {
            Amount = 1,
            PaymentDate = DateTimeOffset.UtcNow,
            CategoryId = 1,
            Category = new Category(1)
            {
                Name = "TestCategory",
                Type = CategoryType.Expenses
            },
            CapitalId = 1,
            Capital = new Capital(1)
            {
                Name = "TestCapital",
                Balance = 0,
                Currency = CurrencyType.UAH
            },
            CreatedBy = "TestUser"
        };
        var entities = new List<Income> { entity };
        var query = new GetIncomesQuery();

        _incomeRepositoryMock.GetAllAsync(new IncomesByQuerySpecification(entity.CreatedBy)).Returns(entities);

        var response = entity.ToResponse();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Incomes.Should().OnlyContain(x => x == response);
    }
}
