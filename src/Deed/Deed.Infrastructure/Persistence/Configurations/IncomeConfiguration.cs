using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.ToTable(TableConfigurationConstants.Incomes);

        builder.HasQueryFilter(c =>
            !c.IsDeleted.HasValue ||
            c.IsDeleted.HasValue && !c.IsDeleted.Value);

        builder.HasIndex(t => t.IsDeleted)
            .HasFilter("is_deleted = 0");

        builder.HasKey(i => i.Id);

        builder.Property(e => e.PaymentDate)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Purpose)
            .HasMaxLength(255);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Capital)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
