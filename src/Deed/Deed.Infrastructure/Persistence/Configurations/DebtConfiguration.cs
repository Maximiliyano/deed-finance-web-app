using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.ToTable(TableConfigurationConstants.Debts);

        builder.Property(d => d.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.HasIndex(d => d.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Item)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(d => d.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.Currency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Source)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(d => d.Recipient)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(d => d.Note)
            .HasMaxLength(512);

        builder.Property(d => d.IsPaid)
            .HasDefaultValue(false);

        builder.HasOne(d => d.Capital)
            .WithMany()
            .HasForeignKey(d => d.CapitalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.OrderIndex)
            .HasDefaultValue(0);

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.UpdatedBy)
            .HasMaxLength(256);
    }
}
