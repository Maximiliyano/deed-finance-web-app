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

        builder.HasQueryFilter(c =>
            !c.IsDeleted.HasValue ||
            c.IsDeleted.HasValue && !c.IsDeleted.Value);

        builder.HasIndex(t => t.IsDeleted)
            .HasFilter("IsDeleted = 0");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentDate)
            .IsRequired();

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18,2);

        builder.Property(e => e.Purpose)
            .HasMaxLength(255);

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
