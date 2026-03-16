using Deed.Application.Tags.Queries.GetAll;
using Deed.Application.Tags.Response;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Tags.Queries;

public sealed class GetAllExpenseTagsQueryHandlerTests
{
    private readonly IExpenseTagRepository _repository = Substitute.For<IExpenseTagRepository>();
    private readonly GetAllExpenseTagsQueryHandler _handler;

    public GetAllExpenseTagsQueryHandlerTests()
    {
        _handler = new GetAllExpenseTagsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsTagList()
    {
        // Arrange
        List<ExpenseTag> tags = new()
        {
            new ExpenseTag { Tag = new Tag { Name = "food" } },
            new ExpenseTag { Tag = new Tag { Name = "transport" } }
        };
        _repository.GetAllAsync(Arg.Any<ISpecification<ExpenseTag>>(), Arg.Any<CancellationToken>())
            .Returns(tags.AsEnumerable());

        // Act
        Result<List<ExpenseTagResponse>> result =
            await _handler.Handle(new GetAllExpenseTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<ISpecification<ExpenseTag>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ExpenseTag>());

        // Act
        Result<List<ExpenseTagResponse>> result =
            await _handler.Handle(new GetAllExpenseTagsQuery("nonexistent"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
