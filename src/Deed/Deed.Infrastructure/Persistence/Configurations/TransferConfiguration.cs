using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable(TableConfigurationConstants.Transfers);
        
        builder.HasKey(t => t.Id);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(x => x.SourceCapital)
            .WithMany(x => x.TransfersOut)
            .HasForeignKey(x => x.SourceCapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationCapital)
            .WithMany(x => x.TransfersIn)
            .HasForeignKey(x => x.DestinationCapitalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
