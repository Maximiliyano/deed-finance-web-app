using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class ExchangeConfiguration : IEntityTypeConfiguration<Exchange>
{
    public void Configure(EntityTypeBuilder<Exchange> builder)
    {
        builder.ToTable(TableConfigurationConstants.Exchanges);
        
        builder.HasKey(ex => ex.Id);

        builder.Property(x => x.NationalCurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.TargetCurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Buy)
            .HasPrecision(18, 4);

        builder.Property(x => x.Sale)
            .HasPrecision(18, 4);

        builder.HasIndex(x => new { x.NationalCurrencyCode, x.TargetCurrencyCode })
            .IsUnique();
    }
}
