using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.HasKey(i => i.Id);

        builder
            .HasOne(i => i.Category)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CategoryId);

        builder
            .Navigation(e => e.Category)
            .AutoInclude();

        builder
            .HasOne(i => i.Capital)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CapitalId);

        builder
            .Navigation(e => e.Capital)
            .AutoInclude();

        builder.ToTable(TableConfigurationConstants.Incomes);
    }
}
