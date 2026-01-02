using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        builder.HasOne(et => et.Expense)
               .WithMany(e => e.Tags)
               .HasForeignKey(et => et.Id);

        builder.HasOne(et => et.Tag)
               .WithMany(t => t.ExpenseTags)
               .HasForeignKey(et => et.TagId);

        builder.HasQueryFilter(et =>
            (!et.IsDeleted.HasValue || et.IsDeleted.HasValue && !et.IsDeleted.Value) &&
            (!et.Expense.IsDeleted.HasValue || et.Expense.IsDeleted.HasValue && !et.Expense.IsDeleted.Value)
        );
    }
}
