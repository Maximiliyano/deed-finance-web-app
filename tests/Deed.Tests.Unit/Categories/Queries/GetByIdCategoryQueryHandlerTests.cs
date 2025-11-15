using Deed.Application.Categories.Queries.GetById;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Categories.Queries;

public sealed class GetByIdCategoryQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock = Substitute.For<ICategoryRepository>();

    private readonly GetByIdCategoryQueryHandler _handler;

    public GetByIdCategoryQueryHandlerTests()
    {
        _handler = new GetByIdCategoryQueryHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldGetCategoryById_WhenCategoryExists_ReturnCategory()
    {
        // Arrange
        const int id = 1;

        var category = new Category(id)
        {
            Name = "Exist category",
            Type = CategoryType.Expenses,
            Expenses = []
        };
        var response = new CategoryResponse(id, category.Name, category.Type, category.Period, category.PlannedPeriodAmount);
        var query = new GetByIdCategoryQuery(id);

        _categoryRepositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);

        await _categoryRepositoryMock.Received(1).GetAsync(Arg.Any<CategoryByIdSpecification>());
    }

    [Fact]
    public async Task Handle_ShouldGetCategoryById_WhenNotFound_ReturnNotFound()
    {
        // Arrange
        const int id = -1;

        var query = new GetByIdCategoryQuery(id);

        _categoryRepositoryMock.GetAsync(Arg.Any<CategoryByIdSpecification>())
            .Returns((Category)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("category"));

        await _categoryRepositoryMock.Received(1).GetAsync(Arg.Any<CategoryByIdSpecification>());
    }
}
