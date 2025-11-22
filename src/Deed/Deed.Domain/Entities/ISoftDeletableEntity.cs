namespace Deed.Domain.Entities;

public interface ISoftDeletableEntity
{
    bool? IsDeleted { get; set; }
}
