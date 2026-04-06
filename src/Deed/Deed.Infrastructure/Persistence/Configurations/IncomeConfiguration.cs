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

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(i => new { i.CreatedBy, i.IsDeleted });
        builder.HasIndex(i => new { i.CapitalId, i.IsDeleted });
        builder.HasIndex(i => new { i.CategoryId, i.IsDeleted });

        builder.HasKey(i => i.Id);

        builder.Property(e => e.PaymentDate)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Purpose)
            .HasMaxLength(255);

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.UpdatedBy)
            .HasMaxLength(256);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Capital)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Tags)
            .WithOne(it => it.Income)
            .HasForeignKey(it => it.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
