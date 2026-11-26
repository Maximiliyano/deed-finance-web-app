using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable(TableConfigurationConstants.BankAccounts);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Provider)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Iban)
            .IsRequired()
            .HasMaxLength(34);

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(8)
            .HasDefaultValue(string.Empty);

        builder.Property(a => a.Name)
            .HasMaxLength(128);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(256);

        builder.HasIndex(a => new { a.CreatedBy, a.Provider, a.Iban })
            .IsUnique();
    }
}
