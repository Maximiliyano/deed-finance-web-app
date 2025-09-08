using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.DataSeed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class CapitalConfiguration : IEntityTypeConfiguration<Capital>
{
    public void Configure(EntityTypeBuilder<Capital> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasData(Seeder.Parse<Capital>(SeederConstants.Capitals));


        builder
            .HasIndex(c => c.Name)
            .IsUnique();

        builder.HasIndex(c => c.Balance);

        builder
            .Property(p => p.RowVersion)
            .IsRowVersion();

        builder
            .Property(c => c.Currency)
            .HasConversion<string>();

        builder
            .HasMany(c => c.Incomes)
            .WithOne(i => i.Capital)
            .HasForeignKey(i => i.CapitalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(c => c.Incomes)
            .AutoInclude();

        builder
            .HasMany(c => c.Expenses)
            .WithOne(e => e.Capital)
            .HasForeignKey(e => e.CapitalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(c => c.Expenses)
            .AutoInclude();

        builder
            .HasMany(c => c.TransfersIn)
            .WithOne(t => t.DestinationCapital)
            .HasForeignKey(t => t.DestinationCapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(c => c.TransfersIn)
            .AutoInclude();

        builder
            .HasMany(c => c.TransfersOut)
            .WithOne(t => t.SourceCapital)
            .HasForeignKey(t => t.SourceCapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(c => c.TransfersOut)
            .AutoInclude();

        builder.ToTable(TableConfigurationConstants.Capitals);
    }
}
