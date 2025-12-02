using Deed.Domain.Constants;
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
        builder.ToTable(TableConfigurationConstants.Capitals);

        builder.HasQueryFilter(c => 
            !c.IsDeleted.HasValue ||
            c.IsDeleted.HasValue && !c.IsDeleted.Value);

        builder.HasIndex(t => t.IsDeleted)
            .HasFilter("IsDeleted = 0");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxLenghtName);

        builder.Property(c => c.Balance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Currency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IncludeInTotal).HasDefaultValue(true);
        builder.Property(x => x.OnlyForSavings).HasDefaultValue(false);
        builder.Property(x => x.OrderIndex).HasDefaultValue(0);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasMany(c => c.Incomes)
            .WithOne(i => i.Capital)
            .HasForeignKey(i => i.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Expenses)
            .WithOne(e => e.Capital)
            .HasForeignKey(e => e.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(c => c.TransfersIn)
            .WithOne(t => t.DestinationCapital)
            .HasForeignKey(t => t.DestinationCapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(c => c.TransfersOut)
            .WithOne(t => t.SourceCapital)
            .HasForeignKey(t => t.SourceCapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(Seeder.Parse<Capital>(SeederConstants.Capitals));
    }
}
