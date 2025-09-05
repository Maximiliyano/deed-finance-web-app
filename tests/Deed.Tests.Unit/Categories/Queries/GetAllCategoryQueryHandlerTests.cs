using Deed.Application.Abstractions.Settings;
using Deed.Application.Categories.Queries.GetAll;
using Deed.Application.Categories.Response;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Queries;

public sealed class GetAllCategoryQueryHandlerTests
{
    private readonly ICategoryRepository _repositoryMock = Substitute.For<ICategoryRepository>();
    private readonly IMemoryCache _memoryCacheMock = Substitute.For<IMemoryCache>();
    private readonly IOptions<MemoryCacheSettings> _settings;

    private readonly GetAllCategoryQueryHandler _handler;

    public GetAllCategoryQueryHandlerTests()
    {
        _settings = Options.Create(new MemoryCacheSettings
        {
            CategoriesTimespanInHours = 1
        });
        _handler = new GetAllCategoryQueryHandler(_settings, _memoryCacheMock, _repositoryMock);
    }

    [InlineData(CategoryType.Expenses)]
    [InlineData(CategoryType.Incomes)]
    [InlineData(null)]
    [Theory]
    public async Task Handle_ShouldGetAllExistingCategories_ReturnsList(CategoryType? type)
    {
        // Arrange
        var query = new GetAllCategoryQuery(type);
        var categories = new List<Category>
        {
            new()
            {
                Name = "Category",
                Type = type ?? CategoryType.Expenses
            }
        };
        var responses = categories.Select(x =>
            new CategoryResponse(x.Id, x.Name, x.Type, x.Period.ToString(), x.PlannedPeriodAmount));


        _repositoryMock.GetAllAsync(type).Returns(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(responses);

        await _repositoryMock.Received(1).GetAllAsync(type);
    }

    [Fact]
    public async Task Handle_ShouldGetAllCategories_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllCategoryQuery();

        _repositoryMock.GetAllAsync(null).Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(Enumerable.Empty<CategoryResponse>());

        await _repositoryMock.Received(1).GetAllAsync(null);
    }
}
