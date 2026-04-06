using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable(TableConfigurationConstants.Expenses);

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.CreatedBy, e.IsDeleted });
        builder.HasIndex(e => new { e.CapitalId, e.IsDeleted });
        builder.HasIndex(e => new { e.CategoryId, e.IsDeleted });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentDate)
            .IsRequired();

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18,2);

        builder.Property(e => e.Purpose)
            .HasMaxLength(255);

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Capital)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CapitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Tags)
            .WithOne(et => et.Expense)
            .HasForeignKey(et => et.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
