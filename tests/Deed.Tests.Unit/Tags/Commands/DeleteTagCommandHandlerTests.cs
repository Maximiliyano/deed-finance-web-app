using Deed.Application.Tags.Commands.Delete;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Tags.Commands;

public sealed class DeleteTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteTagCommandHandler _handler;

    public DeleteTagCommandHandlerTests()
    {
        _handler = new DeleteTagCommandHandler(_tagRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTagExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        Tag tag = new() { Name = "food" };
        _tagRepository.GetAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        Result result = await _handler.Handle(new DeleteTagCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepository.Received(1).Delete(tag);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ReturnsFailure()
    {
        // Arrange
        _tagRepository.GetAsync(Arg.Any<ISpecification<Tag>>(), Arg.Any<CancellationToken>()).Returns((Tag?)null);

        // Act
        Result result = await _handler.Handle(new DeleteTagCommand(99), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _tagRepository.DidNotReceive().Delete(Arg.Any<Tag>());
    }
}
