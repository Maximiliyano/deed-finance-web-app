using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class BudgetEstimationConfiguration : IEntityTypeConfiguration<BudgetEstimation>
{
    public void Configure(EntityTypeBuilder<BudgetEstimation> builder)
    {
        builder.ToTable(TableConfigurationConstants.BudgetEstimations);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.CreatedBy, e.IsDeleted });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.BudgetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.BudgetCurrency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.OrderIndex)
            .HasDefaultValue(0);

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        builder.HasOne(e => e.Capital)
            .WithMany()
            .HasForeignKey(e => e.CapitalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
