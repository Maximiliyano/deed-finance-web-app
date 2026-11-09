namespace Deed.Domain.Entities;

public interface ITagLink : ISoftDeletableEntity
{
    int TagId { get; }

    Tag Tag { get; }
}
