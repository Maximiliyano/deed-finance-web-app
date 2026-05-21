using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class UtilityBillConfiguration : IEntityTypeConfiguration<UtilityBill>
{
    public void Configure(EntityTypeBuilder<UtilityBill> builder)
    {
        builder.ToTable(TableConfigurationConstants.UtilityBills);

        builder.Property(b => b.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasIndex(b => b.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(b => new { b.CreatedBy, b.IsDeleted });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(b => b.EstimatedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.DueDayOfMonth)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.UpdatedBy)
            .HasMaxLength(256);

        builder.HasOne(b => b.Capital)
            .WithMany()
            .HasForeignKey(b => b.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Payments)
            .WithOne(e => e.UtilityBill)
            .HasForeignKey(e => e.UtilityBillId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
