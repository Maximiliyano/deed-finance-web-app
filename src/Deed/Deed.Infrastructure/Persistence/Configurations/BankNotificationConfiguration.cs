using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class BankNotificationConfiguration : IEntityTypeConfiguration<BankNotification>
{
    public void Configure(EntityTypeBuilder<BankNotification> builder)
    {
        builder.ToTable(TableConfigurationConstants.BankNotifications);

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Kind)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Direction)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.AccountLabel)
            .HasMaxLength(64);

        builder.Property(n => n.ExternalId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(n => n.Amount)
            .HasPrecision(18, 2);

        builder.Property(n => n.Currency)
            .HasMaxLength(8);

        builder.Property(n => n.Counterparty)
            .HasMaxLength(256);

        builder.Property(n => n.Purpose)
            .HasMaxLength(512);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.UpdatedBy)
            .HasMaxLength(256);

        builder.HasIndex(n => new { n.CreatedBy, n.ExternalId })
            .IsUnique();

        builder.HasIndex(n => new { n.CreatedBy, n.IsRead, n.OccurredAt });
    }
}
