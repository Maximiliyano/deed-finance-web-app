using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId);

        builder
            .Navigation(e => e.Category)
            .AutoInclude();

        builder
            .HasOne(e => e.Capital)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CapitalId);

        builder
            .Navigation(e => e.Capital)
            .AutoInclude();

        builder.ToTable(TableConfigurationConstants.Expenses);
    }
}
