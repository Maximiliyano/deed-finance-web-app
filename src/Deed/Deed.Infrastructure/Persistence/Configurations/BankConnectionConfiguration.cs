using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class BankConnectionConfiguration : IEntityTypeConfiguration<BankConnection>
{
    public void Configure(EntityTypeBuilder<BankConnection> builder)
    {
        builder.ToTable(TableConfigurationConstants.BankConnections);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => new { c.CreatedBy, c.IsDeleted });

        builder.HasIndex(c => new { c.CreatedBy, c.Provider, c.AccountIban })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => c.Status)
            .HasFilter("[IsDeleted] = 0");

        builder.Property(c => c.Provider)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.EncryptedToken)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(c => c.AccountIban)
            .IsRequired()
            .HasMaxLength(34);

        builder.Property(c => c.ProviderAccountId)
            .HasMaxLength(64);

        builder.Property(c => c.IsExcluded)
            .HasDefaultValue(false);

        builder.Property(c => c.BackfillFailures)
            .HasDefaultValue(0);

        builder.Property(c => c.AutoImport)
            .HasDefaultValue(false);

        builder.Property(c => c.Label)
            .IsRequired()
            .HasMaxLength(64)
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(256);
    }
}
