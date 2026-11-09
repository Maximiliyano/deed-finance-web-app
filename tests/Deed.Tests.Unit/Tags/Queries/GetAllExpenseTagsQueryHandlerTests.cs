using Deed.Application.Auth;
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
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly GetAllExpenseTagsQueryHandler _handler;

    public GetAllExpenseTagsQueryHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new GetAllExpenseTagsQueryHandler(_repository, _user);
    }

    [Fact]
    public async Task Handle_ScopesTagsToTheCurrentUsersExpenses()
    {
        // Arrange
        ISpecification<ExpenseTag>? captured = null;
        _repository.GetAllAsync(Arg.Do<ISpecification<ExpenseTag>>(s => captured = s), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ExpenseTag>());

        // Act
        await _handler.Handle(new GetAllExpenseTagsQuery(), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        var matches = captured!.Criteria!.Compile();

        matches(NewExpenseTag("food", createdBy: "testuser")).Should().BeTrue();
        matches(NewExpenseTag("food", createdBy: "someone-else")).Should().BeFalse();
    }

    private static ExpenseTag NewExpenseTag(string tagName, string createdBy) => new()
    {
        Tag = new Tag { Name = tagName },
        Expense = new Expense { Amount = 1m, PaymentDate = DateTimeOffset.UtcNow, CategoryId = 1, CapitalId = 1, CreatedBy = createdBy }
    };

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
