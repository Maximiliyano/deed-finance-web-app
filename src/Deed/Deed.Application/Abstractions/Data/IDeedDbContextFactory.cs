namespace Deed.Application.Abstractions.Data;

public interface IDeedDbContextFactory
{
    IDeedDbContext CreateReadOnlyContext();
}
