using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseTagConfiguration : IEntityTypeConfiguration<ExpenseTag>
{
    public void Configure(EntityTypeBuilder<ExpenseTag> builder)
    {
        builder.ToTable(TableConfigurationConstants.ExpenseTags);

        builder.HasKey(et => new { et.Id, et.TagId });

        builder.Property(et => et.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(et => et.Expense)
               .WithMany(e => e.Tags)
               .HasForeignKey(et => et.Id);

        builder.HasOne(et => et.Tag)
               .WithMany(t => t.ExpenseTags)
               .HasForeignKey(et => et.TagId);

        builder.HasQueryFilter(et => !et.IsDeleted && !et.Expense.IsDeleted);
    }
}
